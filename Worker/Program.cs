using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PADIMapNoReduce {

    class Program {

        /// <summary>
        /// Application entry point Main
        /// </summary>
        /// <param name="args">ID SERVICE-URL ENTRY-URL</param>
        ///
        static void Main(string[] args) {

            Uri serviceUri = new Uri(args[1]);

            int tcpPort = serviceUri.Port;

            //Create Service on this work
            TcpChannel channel = new TcpChannel(tcpPort);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType( typeof(WorkerServices), "Worker", WellKnownObjectMode.Singleton);

            //Inform JobTracker of this new worker 
            Uri jobTrackerUri;
            bool toBroadcast;

            if (args[2] != null) {                
                jobTrackerUri = new Uri(args[2]);
                toBroadcast = true;
            } else{
                jobTrackerUri = new Uri(args[1]);
                toBroadcast = false;
            }

            TcpChannel jobTrackerChannel = new TcpChannel();
            ChannelServices.RegisterChannel(jobTrackerChannel, toBroadcast);

            IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), args[2]);

            try{
                mt.RegisterNewWorker(args[1], true);
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not locate server");
            }

            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine();

        }
    }


    internal class WorkerServices : MarshalByRefObject, IWorker {

        private byte[] code;
        private string className;

        private List<string> workersUrl;

        public bool SendMapper(byte[] code, string className) {

            Assembly assembly = Assembly.Load(code);

            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes()) {

                if (type.IsClass == true) {

                    if (type.FullName.EndsWith("." + className)) {

                        // create an instance of the object
                        object ClassObj = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        object[] args = new object[] { "testValue" };
                        object resultObject = type.InvokeMember("Map", BindingFlags.Default | BindingFlags.InvokeMethod, null, ClassObj, args);
                        IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>) resultObject;
                        Console.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result) {
                            Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }
                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
            return true;
        }

        public bool RegisterNewWorker(List<string> workersServiceUrl, bool toBroadcast){

            //Add to myself 
            workersUrl.AddRange(workersServiceUrl);

            if (toBroadcast) { //Resend to new worker ther whole workers list

                foreach (string worker in workersUrl) {

                    if (worker == workersServiceUrl[0]) {

                        TcpChannel workerChannel = new TcpChannel();
                        ChannelServices.RegisterChannel(workerChannel, true);

                        IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), worker);

                        try {
                            mt.RegisterNewWorker(workersServiceUrl[0], false);
                        
                        } catch (SocketException) {
                        
                            System.Console.WriteLine("[REGISTERWORKER1] Could not locate server");
                            return false;
                        }

                    } else { 

                        TcpChannel workerChannel = new TcpChannel();
                        ChannelServices.RegisterChannel(workerChannel, true);

                        IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), worker);

                        try {
                            mt.RegisterNewWorker(workersServiceUrl[0], false);
                        
                        } catch (SocketException) {
                        
                            System.Console.WriteLine("[REGISTERWORKER2] Could not locate server");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

    }
}
