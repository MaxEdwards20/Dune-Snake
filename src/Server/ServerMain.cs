namespace Server;

class ServerMain
{
    public static int Main(string[] args)
    {
        if (readArgs(args, out ushort port))
        {
            if (MessageQueueServer.instance.initialize(port))
            {
                Console.WriteLine("Server waiting for incoming connections on port {0}", port);
                startServer();
                MessageQueueServer.instance.shutdown();
            }
            else
            {
                Console.WriteLine("Failure to initialize the MessageQueueServer");
            }
        }
        else
        {
            Console.WriteLine("Invalid port parameter.");
            Console.WriteLine("Example program usage: server --port 4010");
        }

        return 0;
    }


    /// <summary>
    /// Verify the command line port parameter is correct and if so, return the specified port
    /// </summary>
    private static bool readArgs(string[] args, out ushort port)
    {
        Predicate<string> PortParam = (arg => arg == "-p" || arg == "--port" || arg == "-port");

        port = 0;
        bool valid = true;
        if (args.Length != 2)
        {
            valid = false;
        }
        else if (!PortParam(args[0].ToLower()))
        {
            valid = false;
        }
        else
        {
            if (!ushort.TryParse(args[1], out port))
            {
                valid = false;
            }
        }

        return valid;
    }

    private static void startServer()
    {
        TimeSpan SIMULATION_UPDATE_RATE_MS = TimeSpan.FromMilliseconds(33);

        GameModel model = new GameModel();
        bool running = model.initialize();

        DateTime previousTime = DateTime.Now;
        while (running)
        {
            // work out the elapsed time
            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - previousTime;
            previousTime = currentTime;

            // If we are running faster than the simulation update rate, then go to sleep
            // for a bit so we don't burn up the CPU unnecessarily.
            TimeSpan sleepTime = SIMULATION_UPDATE_RATE_MS - elapsedTime;
            if (sleepTime > TimeSpan.Zero)
            {
                //Console.WriteLine("Sleep: {0}", sleepTime.TotalMilliseconds);
                System.Threading.Thread.Sleep(sleepTime);
            }

            // Now, after having slept for a bit, now compute the elapsed time and perform
            // the game model update.
            model.update(elapsedTime);
        }

        model.shutdown();
    }
}