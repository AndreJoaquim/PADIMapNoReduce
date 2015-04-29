using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

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

            //Register new worker in itself
            try{

                IWorker newWorkerObj = (IWorker)Activator.GetObject(typeof(IWorker), serviceUri.ToString());

                newWorkerObj.RegisterOwnWorker(serviceUri.ToString());

            }catch(SocketException e){

                System.Console.WriteLine("[WORKER_MAIN_1]Could not locate server");
                System.Console.WriteLine(e.StackTrace);
                
            }catch(Exception e){

                System.Console.WriteLine("[WORKER_MAIN_2]Could not locate server");
                System.Console.WriteLine(e.StackTrace);
            }

            //If outer jobtracker exists
            if (args.Length >= 3) {

                try{

                    String jobTrackerUri = args[2];

                    IWorker jobTrackerObj = (IWorker)Activator.GetObject(typeof(IWorker), jobTrackerUri);

                    //Broadcast to the network the new worker
                    jobTrackerObj.BroadcastNewWorker(serviceUri.ToString());

                }catch(SocketException e){

                    System.Console.WriteLine("[WORKER_MAIN_3]Could not locate server");
                    System.Console.WriteLine(e.StackTrace);
                
                }catch(Exception e){

                    System.Console.WriteLine("[WORKER_MAIN_4]Could not locate server");
                    System.Console.WriteLine(e.StackTrace);
                
                }
            } 


            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine();

        }
    }


    internal class WorkerServices : MarshalByRefObject, IWorker {

        private byte[] code;
        private string className;

        private string ownUrl;
        private List<string> workersUrl;

        public WorkerServices(){

            workersUrl = new List<string>();
        }


        public bool RequestJob(long inputSize, string className, byte[] dllCode, int NumberOfSplits) {

            //Create Queue with workers
            Queue<string> availableWorker = new Queue<string>();
            availableWorker.Enqueue(ownUrl);

            foreach(string worker in workersUrl)
                availableWorker.Enqueue(worker);
                
            //Split the inputFile between the works
            long splitSize = inputSize / NumberOfSplits;

            Semaphore queueWorkersSemaphore = new Semaphore(workersUrl.Count + 1, workersUrl.Count + 1);

            //Broadcast the job between the whole workers
            for (long i = 0; i < inputSize; i += splitSize) {

                long beginIndex = i;
                long endIndex = i + splitSize - 1;

                //Is last split?
                if (inputSize - i < splitSize){
                    endIndex = inputSize; 
                }

                //Run thread responsible for updating the queue
                
                //Test if there are workers available

                //Send the job to worker


            }

                return true;
        
        }

        public bool SendMapper(byte[] code, string className) {

            //Store code to be executed
            this.code = code;

            //Store className
            this.className = className;

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

        public bool RegisterOwnWorker(string url) {

            ownUrl = url;

            return true;
        }

        public bool BroadcastNewWorker(string url) {

            //Add to own workers list
            workersUrl.Add(url);

            //Send to for each element of workerUrl the new worker Url
            foreach (string worker in workersUrl) {

                IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), worker);

                try {
                    //Create list with the new worker url
                    List<string> workers = new List<string>();
                    workers.Add(url);
                    //Send to an already existing worker the new one
                    workerObj.RegisterNewWorkers(workers);

                } catch (SocketException){

                    System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR1] Could not locate server");
                    return false;

                } catch (Exception e){

                    System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR2]" + e.StackTrace);
                    return false;
                }

            }

            //Send to the new worker a complete list of the workers urls
            IWorker newWorkerObj = (IWorker)Activator.GetObject(typeof(IWorker), url);

            try {
                //Create list with the new worker url
                List<string> workers = new List<string>();
                workers.AddRange(workersUrl);
                //Send to an already existing worker the new one
                newWorkerObj.RegisterNewWorkers(workers);

            }
            catch (SocketException)
            {

                System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR3] Could not locate server");
                return false;

            }
            catch (Exception e)
            {

                System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR4]" + e.StackTrace);
                return false;
            }
            
            return true;
        }

        public bool RegisterNewWorkers(List<string> workerServiceUrl) {

            workersUrl.AddRange(workerServiceUrl);
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
