using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PADIMapNoReduce {

    public class Program {

        /// <summary>
        /// Application entry point Main
        /// </summary>
        /// <param name="args">ID SERVICE-URL $ENTRY-URL</param>
        ///
        public static void Main(string[] args) {

            Uri serviceUri = new Uri(args[1]);

            int tcpPort = serviceUri.Port;

            String[] segments = args[1].Split('/');
            String remoteObjectName = segments[segments.Length - 1];

            //Create Service on this work
            TcpChannel channel = new TcpChannel(tcpPort);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkerServices), remoteObjectName, WellKnownObjectMode.Singleton);

            //Inform JobTracker of this new worker 
            String jobTrackerUri;
            bool toBroadcast;

            if (args.Length >= 3) { //With entry-level                
                jobTrackerUri = args[2];
                toBroadcast = true;
            } else{ //Register itself as job tracket
                jobTrackerUri = args[1];
                toBroadcast = false;
            }
            
            IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), jobTrackerUri);

            try{
                List<string> workers = new List<string>();
                workers.Add(args[1]);
                mt.RegisterNewWorker(workers, toBroadcast);
                mt.PrintStatus();
            }
            catch (SocketException e)
            {
                System.Console.WriteLine("[WORKERMAIN1]Could not locate server");
                System.Console.WriteLine(e.StackTrace);
            }

            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine();

        }
    }


    internal class WorkerServices : MarshalByRefObject, IWorker {

        private byte[] code;
        private string className;

        public List<string> workersUrl;

        public WorkerServices(){

            workersUrl = new List<string>();
        }

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
            throw (new System.Exception("[SENDMAPPER1]could not invoke method"));
            return true;
        }

        public bool RegisterNewWorker(List<string> workersServiceUrl, bool toBroadcast){


            workersUrl.AddRange(workersServiceUrl);

            if (toBroadcast) { //Resend to new worker ther whole workers list

                List<string> tmpWorkersUrl = new List<string>(workersUrl);
                List<string> tmpWorkersServiceUrl = new List<string>(workersServiceUrl);

                foreach (string worker in tmpWorkersUrl) {

                    if (worker.Equals(workersServiceUrl[0])) {//New worker

                        Console.WriteLine("sdas");
                        IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), worker);

                        try
                        {
                            mt.RegisterNewWorker(tmpWorkersUrl, false);

                        }catch (SocketException) {

                            System.Console.WriteLine("[REGISTERWORKER1] Could not locate server");
                            return false;
                        }
                        catch (Exception e){

                            System.Console.WriteLine("[REGISTERWORKER3]" + e.StackTrace);
                        }

                    } else { 

                        IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), worker);

                        try {
                            mt.RegisterNewWorker(tmpWorkersServiceUrl, false);
                        
                        } catch (SocketException){
                        
                            System.Console.WriteLine("[REGISTERWORKER2] Could not locate server");
                            return false;
                        } catch (Exception e){

                            System.Console.WriteLine("[REGISTERWORKER4]" + e.StackTrace);
                        }
                    }
                }
            }

            return true;
        }

        public bool PrintStatus() {

            foreach (string worker in workersUrl)
            {
                Console.WriteLine(worker);
            }

            return true;
        }
    }
}
