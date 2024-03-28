using Shared.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public class MessageQueueClient
    {
        private static MessageQueueClient m_instance;
        private bool m_keepRunning = true;
        private Thread m_threadSender;
        private Thread m_threadReceiver;
        private Socket m_socketServer;

        private Mutex m_mutexSendMessages = new Mutex();
        private ManualResetEvent m_eventSendMessages = new ManualResetEvent(false);
        private ConcurrentQueue<Message> m_sendMessages = new ConcurrentQueue<Message>();
        private Queue<Message> m_sendHistory = new Queue<Message>();
        private uint m_nextMessageId = 0;

        private Queue<Message> m_receivedMessages = new Queue<Message>();
        private Mutex m_mutexReceivedMessages = new Mutex();

        private delegate Message TypeToMessage();
        private Dictionary<Shared.Messages.Type, TypeToMessage> m_messageCommand = new Dictionary<Shared.Messages.Type, TypeToMessage>();

        public static MessageQueueClient instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MessageQueueClient();
                }
                return m_instance;
            }
        }

        /// <summary>
        /// Create two threads for the message queue:
        ///  1. Listen for incoming messages
        ///  2. Sending messages
        /// </summary>
        public bool initialize(string address, ushort port)
        {
            IPAddress ipAddress = parseIPAddress(address);
            IPEndPoint endpoint = new IPEndPoint(ipAddress, port);

            m_socketServer = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_messageCommand[Shared.Messages.Type.ConnectAck] = () => { return new ConnectAck(); };
            m_messageCommand[Shared.Messages.Type.NewEntity] = () => { return new NewEntity(); };
            m_messageCommand[Shared.Messages.Type.UpdateEntity] = () => { return new UpdateEntity(); };
            m_messageCommand[Shared.Messages.Type.RemoveEntity] = () => { return new RemoveEntity(); };

            try
            {
                m_socketServer.Connect(endpoint);
                initializeSender();
                initializeReceiver();
            }
            catch (SocketException e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gracefully shutdown the network connection and related activities
        /// </summary>
        public void shutdown()
        {
            m_keepRunning = false;
            m_eventSendMessages.Set();
            m_socketServer.Shutdown(SocketShutdown.Both);
            m_socketServer.Disconnect(false);
            m_socketServer.Close();
        }

        /// <summary>
        /// Two steps in sending a message:
        ///  1. Add the message the the message queue
        ///  2. Signal the thread that performs the sending that a new message is available
        /// </summary>
        public void sendMessage(Message message)
        {
            m_sendMessages.Enqueue(message);
            m_eventSendMessages.Set();
        }

        /// <summary>
        /// Some messages need to be sent with a sequence number, when that
        /// is needed, this send method is used.
        /// </summary>
        public void sendMessageWithId(Message message)
        {
            message.messageId = m_nextMessageId++;
            sendMessage(message);
        }

        /// <summary>
        /// Returns the queue of all messages received since the last time
        /// this method was called.
        /// </summary>
        public Queue<Message>? getMessages()
        {
            if (m_receivedMessages.Count  == 0)
            {
                return null;
            }

            Queue<Message> empty = new Queue<Message>();
            Queue<Message> previous = m_receivedMessages;

            lock (m_mutexReceivedMessages)
            {
                m_receivedMessages = empty;
            }

            return previous;
        }

        /// <summary>
        /// Removes all messages up to and including the lastMessageId, and
        /// then returns a copy of the remaining messages.  This is used
        /// during server reconciliation.
        /// </summary>
        public Queue<Message> getSendMessageHistory(uint lastMessageId)
        {
            lock (m_sendHistory)
            {
                // Remove messages up to and including lastMessageId
                while (m_sendHistory.Count > 0 && m_sendHistory.Peek().messageId.Value <= lastMessageId)
                {
                    m_sendHistory.Dequeue();
                }

                // Make a copy of the remaining queue and return that
                return new Queue<Message>(m_sendHistory);
            }
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
                while (m_keepRunning)
                {
                    if (m_sendMessages.TryDequeue(out var item))
                    {
                        // Need to track messages with a sequence number for server reconciliation
                        if (item.messageId.HasValue)
                        {
                            lock (m_sendHistory)
                            {
                                m_sendHistory.Enqueue(item);
                            }
                        }

                        // Three items are sent: type, size, message body
                        byte[] type = BitConverter.GetBytes((UInt16)item.type);
                        byte[] body = item.serialize();
                        byte[] size = BitConverter.GetBytes(body.Length);

                        // Type
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(type);
                        }
                        m_socketServer.Send(type);

                        // Size
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(size);
                        }
                        m_socketServer.Send(size);

                        // Message body
                        m_socketServer.Send(body);
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
        // Set's up a thread that listens for incoming messages on the socket
        // to the server.  If there is something to receive, the message is
        // read, parsed, and added to the queue of received messages.
        /// </summary>
        private void initializeReceiver() 
        {
            m_threadReceiver = new Thread(() =>
            {
                byte[] type = new byte[sizeof(Shared.Messages.Type)];
                byte[] size = new byte[sizeof(int)];

                m_socketServer.ReceiveTimeout = 100;    // milliseconds

                while (m_keepRunning)
                {
                    try
                    {
                        // By definition, the first of the three messages is the message type
                        int bytesReceived = m_socketServer.Receive(type);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(type);
                        }
                        if (bytesReceived > 0)
                        {
                            // Read the size of the message body
                            m_socketServer.Receive(size);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(size);
                            }

                            // Read the message body
                            byte[] body = new byte[BitConverter.ToInt32(size)];
                            m_socketServer.Receive(body);
                            // Deserialize the bytes into the actual message
                            Message message = m_messageCommand[(Shared.Messages.Type)BitConverter.ToUInt16(type)]();
                            message.parse(body);

                            lock (m_mutexReceivedMessages)
                            {
                                m_receivedMessages.Enqueue(message);
                            }
                        }
                    }
                    catch (SocketException)
                    {
                        // We expect this when a timeout occurs on the receiver
                    }

                }
            });
            m_threadReceiver.Start();
        }

        private static IPAddress parseIPAddress(string address)
        {
            IPAddress ipAddress;
            if (address == "localhost")
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHost.AddressList[0];
            }
            else
            {
                ipAddress = IPAddress.Parse(address);
            }

            return ipAddress;
        }
    }
}
