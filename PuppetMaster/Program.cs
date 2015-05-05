using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace PuppetMaster
{
    class Program
    {

        // MACROS
        public const string CLIENT_URI = "CLIENT_URI";
        public const string PUPPET_MASTER_PORT = "PM_PORT";

        // Commands
        public const string WORKER = "WORKER";
        public const string SUBMIT = "SUBMIT";
        public const string WAIT = "WAIT";
        public const string STATUS = "STATUS";
        public const string SLOWW = "SLOWW";
        public const string FREEZEW = "FREEZEW";
        public const string UNFREEZEW = "UNFREEZEW";
        public const string FREEZEC = "FREEZEC";
        public const string UNFREEZEC = "UNFREEZEC";

        // Used for Debug purposes
        public const string SETUP = "SETUP";

        static void Main(string[] args)
        {

            String url = "";

            // Get the IP's host
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    url = ip.ToString();
                }
            }

            CreateClient(url);

            String remoteObjectName = "PM";

            // Prepend the protocol and append the port
            int tcpPort = NextFreeTcpPort(20001,29999);
            url = "tcp://" + url + ":" + tcpPort + "/" + remoteObjectName;

            System.Environment.SetEnvironmentVariable(PUPPET_MASTER_PORT, tcpPort.ToString(), EnvironmentVariableTarget.Process);

            // Register the Puppet Master Service
            TcpChannel channel = new TcpChannel(tcpPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMasterImplementation), remoteObjectName, WellKnownObjectMode.Singleton);

            // Console message
            Console.WriteLine("Created PuppetMaster at " + url + ".");

            IPuppetMaster puppetMaster = (IPuppetMaster) Activator.GetObject(typeof(IPuppetMaster), url);

            while (true) {

                string input = Console.ReadLine();
                string[] split = input.Split(' ');

                string command = split[0];

                switch (command) {

                    case SETUP:

                        Console.WriteLine("-- Running setup...");

                        // Create WORKER 1
                        puppetMaster.CreateWorker("1", "tcp://localhost:20001/PM", "tcp://localhost:30001/W");
                        Thread.Sleep(4000);
                        // Create WORKER 2
                        puppetMaster.CreateWorker("2", "tcp://localhost:20001/PM", "tcp://localhost:30002/W", "tcp://localhost:30001/W");
                        Thread.Sleep(4000);
                        // Create WORKER 3
                        puppetMaster.CreateWorker("3", "tcp://localhost:20001/PM", "tcp://localhost:30003/W", "tcp://localhost:30002/W");
                        Thread.Sleep(4000);
                        // Create WORKER 4
                        puppetMaster.CreateWorker("4", "tcp://localhost:20001/PM", "tcp://localhost:30004/W", "tcp://localhost:30002/W");

                        Console.WriteLine("-- Setup concluded!");

                        break;
                
                    case WORKER:

                        /*
                         * WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>
                         */

                        if(split.Length <= 5){
                
                        //Create Client

                            string id = split[1];
                            string puppetMasterUrl = split[2];
                            string serviceUrl = split[3];

                            if (split.Length == 5) {
                                string entryUrl = split[4];
                                puppetMaster.CreateWorker(id, puppetMasterUrl, serviceUrl, entryUrl);
                            } else {
                                puppetMaster.CreateWorker(id, puppetMasterUrl, serviceUrl);
                            }

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>");
                        }

                        break;

                    case SUBMIT:

                        /*
                         * SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> 
                         */

                        if (split.Length == 7)
                        {

                            // Get the arguments
                            string entryUrl = split[1];
                            string file = split[2];
                            string output = split[3];
                            string className = split[4];
                            string classImplementationPath = split[5];
                            int numberOfSplits = int.Parse(split[6]);

                            // Probably not the best approach...
                            // Our test files and output directory are in the Puppet Master .exe's directory
                            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\files\";
                            file = path + file;
                            output = path + output;
                            classImplementationPath = path + classImplementationPath;

                            // Submit job to the puppetMaster Impl
                            puppetMaster.SubmitJob(entryUrl, file, output, className, classImplementationPath, numberOfSplits);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP>");
                        }

                        break;

                    case WAIT:

                        /*
                          * WAIT <SECS> 
                          */

                        if (split.Length == 2)
                        {

                            int secs = Int32.Parse(split[1]);
                            puppetMaster.Wait(secs);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: WAIT <SECS>");
                        }

                        break;

                    case STATUS:

                        /*
                          * STATUS 
                          */

                        puppetMaster.Status();

                        break;

                    case SLOWW:

                        /*
                          * SLOWW <ID> <delay-in-seconds>
                          */

                        if (split.Length == 3)
                        {

                            string id = split[1];
                            int delayInSeconds = Int32.Parse(split[2]);

                            puppetMaster.SlowW(id, delayInSeconds);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SLOWW <ID> <delay-in-seconds>");
                        }

                        break;

                    case FREEZEW:

                        /*
                          * SLOWW <ID> <delay-in-seconds>
                          */

                        if (split.Length == 3)
                        {

                            string id = split[1];

                            puppetMaster.FrezeeW(id);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SLOWW <ID> <delay-in-seconds>");
                        }

                        break;

                    case UNFREEZEW:

                        /*
                          * SLOWW <ID> <delay-in-seconds>
                          */

                        if (split.Length == 3)
                        {

                            string id = split[1];
                            int delayInSeconds = Int32.Parse(split[2]);

                            puppetMaster.SlowW(id, delayInSeconds);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SLOWW <ID> <delay-in-seconds>");
                        }

                        break;

                    case FREEZEC:

                        /*
                          * SLOWW <ID> <delay-in-seconds>
                          */

                        if (split.Length == 3)
                        {

                            string id = split[1];
                            int delayInSeconds = Int32.Parse(split[2]);

                            puppetMaster.SlowW(id, delayInSeconds);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SLOWW <ID> <delay-in-seconds>");
                        }

                        break;

                    case UNFREEZEC:

                        /*
                          * SLOWW <ID> <delay-in-seconds>
                          */

                        if (split.Length == 3)
                        {

                            string id = split[1];
                            int delayInSeconds = Int32.Parse(split[2]);

                            puppetMaster.SlowW(id, delayInSeconds);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SLOWW <ID> <delay-in-seconds>");
                        }

                        break;
                
                }

            }

        }

        private static bool CreateClient(String url) {

            String clientUrl = "";
            int port = NextFreeTcpPort(10001, 19999);
            // Prepend the protocol and append the port
            clientUrl = "tcp://" + url + ":" + port + "/C";

            System.Environment.SetEnvironmentVariable(CLIENT_URI, clientUrl, EnvironmentVariableTarget.Process);

            // Start the worker process
            string clientExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Client\\bin\\Debug\\Client.exe");
            Process.Start(clientExecutablePath, clientUrl);

            return true;
        
        }

        /* 
         * Utility functions
         */
        private static int NextFreeTcpPort(int lowerBound, int highBound) {

            int portStartIndex = lowerBound;
            int portEndIndex = highBound;

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = portStartIndex; port <= portEndIndex; port++) {
                if (!usedPorts.Contains(port)) {
                    unusedPort = port;
                    break;
                }
            }

            return unusedPort;

        }

        

    }
}
