using Shared.Messages;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class MessageQueueServer
    {
        private static MessageQueueServer m_instance;
        private bool m_keepRunning = true;
        private Thread m_threadListener;
        private Thread m_threadSender;
        private Thread m_threadReceiver;
        private Socket m_listener;
        private Dictionary<int, Socket> m_clients = new Dictionary<int, Socket>();
        private Mutex m_mutexSockets = new Mutex();

        private ConcurrentQueue<Tuple<int, uint?, Message>> m_sendMessages = new ConcurrentQueue<Tuple<int, uint?, Message>>();
        private ManualResetEvent m_eventSendMessages = new ManualResetEvent(false);
        private Mutex m_mutexSendMessages = new Mutex();

        private Queue<Tuple<int, Message>> m_receivedMessages = new Queue<Tuple<int, Message>>();
        private Mutex m_mutexReceivedMessages = new Mutex();
        private Dictionary<int, uint> m_clientLastMessageId = new Dictionary<int, uint>();

        private delegate Message TypeToMessage();
        private Dictionary<Shared.Messages.Type, TypeToMessage> m_messageCommand = new Dictionary<Shared.Messages.Type, TypeToMessage>();

        public delegate void ConnectHandler(int clientId);
        public delegate void DisconnectHandler(int clientId);
        private ConnectHandler m_connectHandler;
        private DisconnectHandler m_disconnectHandler;
        public static MessageQueueServer instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MessageQueueServer();
                }
                return m_instance;
            }
        }

        /// <summary>
        /// Create three threads for the message queue:
        ///  1. Listen for incoming client connections
        ///  2. Listen for incoming messages
        ///  3. Sending of messages
        /// </summary>
        public bool initialize(ushort port)
        {
            m_messageCommand[Shared.Messages.Type.Join] = () => { return new Join(); };
            m_messageCommand[Shared.Messages.Type.Input] = () => { return new Input(); };
            m_messageCommand[Shared.Messages.Type.Disconnect] = () => { return new Disconnect(); };

            initializeListener(port);
            initializeSender();
            initializeReceiver();

            return true;
        }

        /// <summary>
        /// Gracefully shut things down
        /// </summary>
        public void shutdown()
        {
            m_keepRunning = false;
            m_eventSendMessages.Set();
            foreach (var socket in m_clients.Values)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
            }
            m_listener.Close();
        }

        /// <summary>
        /// Two steps in sending a message:
        ///  1. Add the message the the message queue
        ///  2. Signal the thread that performs the sending that a new message is available
        /// </summary>
        public void sendMessage(int clientId, Message message, uint? messageId = null)
        {
            // The reason messageId is part of the tuple rather than directly setting it on
            // the message at this point is that broadcast messages are not copies, but we
            // want each message that goes out to a client to have the unique per-client
            // last message sequence number attached to it.  Right before the message
            // is sent in the sender thread, the messageId is set on the message, ensure
            // the correct sequence number is sent to the client.
            m_sendMessages.Enqueue(Tuple.Create(clientId, messageId, message));
            m_eventSendMessages.Set();
        }

        /// <summary>
        /// Some messages go back to the client with the id of the last message
        /// processed by the server.  This is for use in client-side server
        /// reconciliation.
        /// </summary>
        public void sendMessageWithLastId(int clientId, Message message)
        {
            var lastId = m_clientLastMessageId.ContainsKey(clientId) ? m_clientLastMessageId[clientId] : 0;
            sendMessage(clientId, message, lastId);
        }

        /// <summary>
        /// Send the message to all connected clients.
        /// </summary>
        /// <param name="message"></param>
        public void broadcastMessage(Message message)
        {
            lock (m_mutexSockets)
            {
                foreach (var clientId in m_clients.Keys)
                {
                    sendMessage(clientId, message);
                }
            }
        }

        /// <summary>
        /// Send the message to all connected clients, but also including the
        /// last message sequence number processed by the server.
        /// </summary>
        public void broadcastMessageWithLastId(Message message)
        {
            lock(m_mutexSockets)
            {
                foreach (var clientId in m_clients.Keys)
                {
                    sendMessageWithLastId(clientId, message);
                }
            }
        }

        /// <summary>
        /// Returns the queue of all messages received since the last time
        /// this method was called.
        /// </summary>
        public Queue<Tuple<int, Message>>? getMessages()
        {
            if (m_receivedMessages.Count == 0)
            {
                return null;
            }

            Queue<Tuple<int, Message>> empty = new Queue<Tuple<int, Message>>();
            Queue<Tuple<int, Message>> previous = m_receivedMessages;

            lock (m_mutexReceivedMessages)
            {
                m_receivedMessages = empty;
            }

            return previous;
        }

        public void registerConnectHandler(ConnectHandler handler)
        {
            m_connectHandler = handler;

        }

        /// <summary>
        /// Listen for incomming client connections.  As a connectin is made
        /// remember it and begin listening for messages over that socket.
        /// </summary>
        private void initializeListener(ushort port)
        {
            // Get the ip address of the host (the machine running this program)
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.IPv6Any;
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            // Make a listener socket on this host and bind it to the endpoint
            // SocketType.Stream is the type we use for TCP
            m_listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_listener.Bind(endPoint);

            // Set the socket to listen, with a waiting queue of up to 10 connections
            m_listener.Listen(10);

            m_threadListener = new Thread(() =>
            {
                while (m_keepRunning)
                {
                    Socket client = m_listener.Accept();
                    lock (m_listener)
                    {
                        lock (m_mutexSockets)
                        {
                            m_clients.Add(client.GetHashCode(), client);
                        }
                        Console.WriteLine("Client connected from {0}", client.RemoteEndPoint.ToString());
                    }
                    m_connectHandler(client.GetHashCode());
                }
            });
            m_threadListener.Start();
        }

        /// <summary>
        /// Prepares the message queue for sending of messages.  As messages
        /// are added to the queue of messages to send, the thread created
        /// in this method sends them as soon as it can.
        /// </summary>
        private void initializeSender()
        {
            m_threadSender = new Thread(() =>
            {
                List<int> remove = new List<int>();
                while (m_keepRunning)
                {
                    if (m_sendMessages.TryDequeue(out var item))
                    {
                        // Some messages have a sequence number associated with them, if they do,
                        // then set it on the message.
                        if (item.Item2.HasValue)
                        {
                            item.Item3.messageId = item.Item2.Value;
                        }
                        lock (m_mutexSockets)
                        {
                            if (m_clients.ContainsKey(item.Item1))
                            {
                                try
                                {
                                    Console.WriteLine("Sending message {0} to {1}", item.Item3.type, item.Item1);
                                    // Three items are sent: type, size, message body
                                    byte[] type = BitConverter.GetBytes((UInt16)item.Item3.type);
                                    byte[] body = item.Item3.serialize();
                                    byte[] size = BitConverter.GetBytes(body.Length);

                                    // Type
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        Array.Reverse(type);
                                    }
                                    m_clients[item.Item1].Send(type);

                                    // Size
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        Array.Reverse(size);
                                    }
                                    m_clients[item.Item1].Send(size);

                                    // Message body
                                    m_clients[item.Item1].Send(body);
                                }
                                catch (SocketException)
                                {
                                    Console.WriteLine("Client {0} disconnected", item.Item1.GetHashCode());
                                    m_clients[item.Item1].Disconnect(false);
                                    m_clients[item.Item1].Close();
                                    remove.Add(item.Item1);
                                }
                            }
                        }

                        lock (m_mutexSockets)
                        {
                            foreach (var clientId in remove)
                            {
                                m_clients.Remove(clientId);
                                m_clientLastMessageId.Remove(clientId);
                            }
                            remove.Clear();
                        }
                    }
                    else
                    {
                        lock (m_mutexSendMessages)
                        {
                            m_eventSendMessages.WaitOne();
                            m_eventSendMessages.Reset();
                        }
                    }
                }
            });
            m_threadSender.Start();
        }

        /// <summary>
        /// Sets up a thread that listens for incoming messages on all
        /// known client sockets.  If there is something to receive on a
        /// socket, the message is read, parsed, and added to the queue
        /// of received messages.
        /// </summary>
        private void initializeReceiver()
        {
            m_threadReceiver = new Thread(() =>
            {
                byte[] type = new byte[sizeof(Shared.Messages.Type)];
                byte[] size = new byte[sizeof(int)];

                List<int> remove = new List<int>();
                // TODO: This is a busy loop, would be nice to efficiently wait for an incoming message
                //       from a client
                while (m_keepRunning)
                {
                    lock (m_listener)
                    {
                        lock (m_mutexSockets)
                        {
                            foreach (var client in m_clients)
                            {
                                try
                                {
                                    if (client.Value.Connected && client.Value.Available > 0)
                                    {
                                        // Read the type first
                                        client.Value.Receive(type);
                                        if (BitConverter.IsLittleEndian)
                                        {
                                            Array.Reverse(type);
                                        }

                                        // Read the size of the message body
                                        client.Value.Receive(size);
                                        if (BitConverter.IsLittleEndian)
                                        {
                                            Array.Reverse(size);
                                        }

                                        // Read the message body
                                        byte[] body = new byte[BitConverter.ToInt32(size)];
                                        client.Value.Receive(body);

                                        // Deserialize the bytes into the actual message
                                        Message message = m_messageCommand[(Shared.Messages.Type)BitConverter.ToUInt16(type)]();
                                        message.parse(body);
                                        if (message.messageId.HasValue)
                                        {
                                            m_clientLastMessageId[client.Key] = message.messageId.Value;
                                        }
                                        lock (m_mutexReceivedMessages)
                                        {
                                            m_receivedMessages.Enqueue(new Tuple<int, Message>(client.Key, message));
                                        }

                                        Console.WriteLine("Received message {0} from {1}", message.type, client.Key);
                                    }

                                }
                                catch (SocketException)
                                {
                                    Console.WriteLine("Client {0} disconnected - here", client.Key);
                                    remove.Add(client.Key);
                                }
                            }
                        }

                        lock (m_mutexSockets)
                        {
                            foreach (var clientId in remove)
                            {
                                m_clients.Remove(clientId);
                            }
                        }

                        // Have to do this in a different scope from the mutex lock above
                        // because the disconnect handler calls back into the message queue
                        // to broadcast messages.  This is bad, bad, because it means this code
                        // knows something about the implementation of the disconnect handler.
                        // I'll keep thinking about this to find a better overall solution.
                        foreach (var clientId in remove)
                        {
                            m_disconnectHandler(clientId);
                        }
                        remove.Clear();
                    }
                }
            });
            m_threadReceiver.Start();
        }
    }
}
