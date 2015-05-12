using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster {
    public partial class ScriptLoader : Form {

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

        String url;

        public ScriptLoader() {
            InitializeComponent();

            url = "";

            // Get the IP's host
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    url = ip.ToString();
                }
            }

            CreateClient(url);

            String remoteObjectName = "PM";

            // Prepend the protocol and append the port
            int tcpPort = NextFreeTcpPort(20001, 29999);
            url = "tcp://" + url + ":" + tcpPort + "/" + remoteObjectName;

            System.Environment.SetEnvironmentVariable(PUPPET_MASTER_PORT, tcpPort.ToString(), EnvironmentVariableTarget.Process);

            // Register the Puppet Master Service
            TcpChannel channel = new TcpChannel(tcpPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMasterImplementation), remoteObjectName, WellKnownObjectMode.Singleton);

            // Console message
            Console.WriteLine("Created PuppetMaster at " + url + ".");


        }

        private void bt_loadScript_Click(object sender, EventArgs e) {

            String inputFilePath = "";

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = Application.StartupPath;

            if (dialog.ShowDialog() == DialogResult.OK) {

                inputFilePath = dialog.FileName;
            }

            //Some kinda of loop

            using (StreamReader sr = new StreamReader(inputFilePath)) {

                String line;

                while ((line = sr.ReadLine()) != null) {

                    if(line != "")
                        HandleCommand(line, url);

                }
            }


        }

        private static void HandleCommand(String input, String url) {

            IPuppetMaster puppetMaster = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), url);

            string[] split = input.Split(' ');

            if (split[split.Count() - 1] == "") {
                split = split.Take(split.Count() - 1).ToArray();  
            }

                string command = split[0];
                

                switch (command) {

                   case WORKER:

                        /*
                         * WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>
                         */

                        if (split.Length <= 5) {

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

                        if (split.Length == 7) {

                            // Get the arguments
                            string entryUrl = split[1];
                            string file = split[2];
                            string output = split[3];
                            int numberOfSplits = int.Parse(split[4]);
                            string className = split[5];
                            string classImplementationPath = split[6];

                            // Probably not the best approach...
                            // Our test files and output directory are in the Puppet Master .exe's directory inside folder "files"
                            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                            file = Path.Combine(path, file);
                            output = Path.Combine(path, output);

                            // Create the directory if it doesn't exist
                            if (!Directory.Exists(output)) {
                                Directory.CreateDirectory(output);
                                Console.WriteLine("[SUBMIT] Created output directory at {0}", output);
                            }

                            classImplementationPath = Path.Combine(path, classImplementationPath);

                            // Submit job to the puppetMaster Impl

                            try {
                                puppetMaster.SubmitJob(entryUrl, file, output, className, classImplementationPath, numberOfSplits);
                            } catch (Exception exception) {
                                System.Console.WriteLine(exception.StackTrace);
                                System.Console.ReadLine();
                            }
                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> <DLL>");
                        }

                        break;

                    case WAIT:

                        /*
                          * WAIT <SECS> 
                          */

                        if (split.Length == 2) {

                            int secs = Int32.Parse(split[1]);
                            puppetMaster.Wait(secs);

                        } else {
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

                        if (split.Length == 3) {

                            string id = split[1];
                            int delayInSeconds = Int32.Parse(split[2]);

                            puppetMaster.SlowW(id, delayInSeconds);

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SLOWW <ID> <delay-in-seconds>");
                        }

                        break;

                    case FREEZEW:

                        /*
                          * FREEZEW <ID>
                          */

                        if (split.Length == 2) {

                            string id = split[1];

                            puppetMaster.FrezeeW(id);

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: FREEZEW <ID>");
                        }

                        break;

                    case UNFREEZEW:

                        /*
                          * UNFREEZEW <ID>
                          */

                        if (split.Length == 2) {

                            string id = split[1];

                            puppetMaster.UnfreezeW(id);

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: UNFREEZEW <ID>");
                        }

                        break;

                    case FREEZEC:

                        /*
                          * FREEZEC <ID>
                          */

                        if (split.Length == 2) {

                            string id = split[1];

                            puppetMaster.FreezeC(id);

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: FREEZEC <ID>");
                        }

                        break;

                    case UNFREEZEC:

                        /*
                          * UNFREEZEC <ID>
                          */

                        if (split.Length == 2) {

                            string id = split[1];

                            puppetMaster.UnfreezeC(id);

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: UNFREEZEC <ID>");
                        }

                        break;

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

        private void bt_execute_Click(object sender, EventArgs e) {

            HandleCommand(tb_command.Text, url);
        }

    }
}

