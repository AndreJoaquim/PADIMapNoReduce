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

            int id = int.Parse(args[0]);

            String[] segments = args[1].Split('/');
            String remoteObjectName = segments[segments.Length - 1];

            //Create Service on this work
            TcpChannel channel = new TcpChannel(tcpPort);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkerServices), remoteObjectName, WellKnownObjectMode.Singleton);

            try {

                IWorker newWorkerObj = (IWorker) Activator.GetObject(typeof(IWorker), serviceUri.ToString());

                // Register new worker in itself passing the ID and the URI
                newWorkerObj.RegisterOwnWorker(id, serviceUri.ToString());

            } catch(SocketException e){

                System.Console.WriteLine("[WORKER_ERROR1:MAIN] Could not locate server");
                System.Console.WriteLine(e.StackTrace);
                
            } catch(Exception e){

                System.Console.WriteLine("[WORKER_ERROR2:MAIN] Could not locate server");
                System.Console.WriteLine(e.StackTrace);
            }

            // If outer jobtracker exists
            if (args.Length >= 3) {

                try {

                    String jobTrackerUri = args[2];
                    IWorker jobTrackerObj = (IWorker) Activator.GetObject(typeof(IWorker), jobTrackerUri);
                    // Broadcast to the network the new worker
                    jobTrackerObj.BroadcastNewWorker(serviceUri.ToString());

                } catch(SocketException e){

                    System.Console.WriteLine("[WORKER_ERROR3:MAIN] Could not locate server");
                    System.Console.WriteLine(e.StackTrace);
                
                } catch(Exception e){

                    System.Console.WriteLine("WORKER_ERROR4:MAIN] Could not locate server");
                    System.Console.WriteLine(e.StackTrace);
                
                }
            }

            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine();

        }

    }

    internal class WorkerServices : MarshalByRefObject, IWorker {

        private string ownUrl;
        private List<string> workersUrl;

        private Queue<string> availableWorkerToJob;
        private Semaphore queueWorkersSemaphore;

        private int mId;

        public WorkerServices(){

            workersUrl = new List<string>();

        }

        public bool RequestJob(string clientUrl, long inputSize, string className, byte[] dllCode, int NumberOfSplits) {

            System.Console.WriteLine("[REQUEST_JOB] Broadcasting Job...");

            System.Console.WriteLine("[REQUEST_JOB] Create Workers Queue");

            // Create Queue with workers
            availableWorkerToJob = new Queue<string>();
            availableWorkerToJob.Enqueue(ownUrl);

            foreach(string worker in workersUrl)
                availableWorkerToJob.Enqueue(worker);
                
            //Split the inputFile between the works
            long splitSize = inputSize / NumberOfSplits;
            long remainder = inputSize % NumberOfSplits;

            queueWorkersSemaphore = new Semaphore(workersUrl.Count + 1, workersUrl.Count + 1);


            //Broadcast the job between the whole workers
            for (long i = 0; i < inputSize; i += splitSize) {

                long beginIndex = i;
                long endIndex = beginIndex + splitSize - 1;
                
                //Is last split?
                if (beginIndex + splitSize + remainder >= inputSize)
                {
                    endIndex += remainder + 1;
                    i += remainder + 1;
                }

                //Test if there are workers available
                queueWorkersSemaphore.WaitOne();
                
                //Send the job to worker
                String workerUrl = availableWorkerToJob.Dequeue();

                System.Console.WriteLine("[REQUEST_JOB] Send work to " + workerUrl + "...");

                IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), workerUrl);

                workerObj.RunJob(className, dllCode, beginIndex, endIndex, clientUrl, ownUrl);

                System.Console.WriteLine("[REQUEST_JOB] Sent work to " + workerUrl);
            }
            
            return true;
        
        }

        public bool FinishProcessing(string workerUrl) {

            availableWorkerToJob.Enqueue(workerUrl);

            queueWorkersSemaphore.Release(1);

            return true;
        }

        public bool RunJob(string className, byte[] dllCode, long beginIndex, long endIndex, string clientUrl, string jobTackerUrl) {

            System.Console.WriteLine("[RUN_JOB] Running job...");

            System.Console.WriteLine("[RUN_JOB] Get input splits.");

            // Get input split
            IClient clientObj = (IClient)Activator.GetObject(typeof(IClient), clientUrl);

            System.Console.WriteLine("[RUN_JOB] Connected to client at {0}!", clientUrl);
            
            string input = clientObj.getInputSplit(mId, beginIndex, endIndex);

            System.Console.WriteLine("[RUN_JOB] Load assembly code.");

            Assembly assembly = Assembly.Load(dllCode);

            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes()) {

                if (type.IsClass == true) {

                    if (type.FullName.EndsWith("." + className)) {

                        try {

                            System.Console.WriteLine("[RUN_JOB] Create running instance");

                            // create an instance of the object
                            object ClassObj = Activator.CreateInstance(type);

                            // Dynamically Invoke the method
                            object[] args = new object[] { input };

                            System.Console.WriteLine("[RUN_JOB] Run method");
                            object resultObject = type.InvokeMember("Map", BindingFlags.Default | BindingFlags.InvokeMethod, null, ClassObj, args);
                            IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>)resultObject;

                            Console.WriteLine("Map call result was: ");

                            foreach (KeyValuePair<string, string> p in result) {
                                Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                            }

                            clientObj.sendProcessedSplit(mId, result);

                            IWorker jobTracker = (IWorker)Activator.GetObject(typeof(IWorker), jobTackerUrl);

                            jobTracker.FinishProcessing(ownUrl);

                            return true;

                        } catch (Exception e) {

                            System.Console.WriteLine("[WORKER_SERVICES_ERROR1:RUN_JOB] Could not invoke method:");
                            System.Console.WriteLine(e.StackTrace);

                        }
                       

                    }

                }

            }

            System.Console.WriteLine("[RUN_JOB_2]Could not invoke method:");

            return false;

        }

        public bool RegisterOwnWorker(int id, string url) {

            ownUrl = url;
            mId = id;

            return true;
        }

        public bool BroadcastNewWorker(string url) {

            //Add to own workers list
            workersUrl.Add(url);

            //Send to for each element of workerUrl the new worker Url
            foreach (string worker in workersUrl) {

                if(!worker.Equals(url)){

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

                        System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR2] " + e.StackTrace);
                        return false;
                    }

                }

            }

            //Send to the new worker a complete list of the workers urls
            IWorker newWorkerObj = (IWorker) Activator.GetObject(typeof(IWorker), url);

            try {
                //Create list with the new worker url
                List<string> workers = new List<string>();
                workers.Add(ownUrl);
                workers.AddRange(workersUrl);
                workers.Remove(url);
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

            Console.WriteLine("STATUS: ");
            Console.WriteLine("My Url: " + ownUrl);

            Console.WriteLine("Connections: ");

            foreach (string worker in workersUrl)
            {
                Console.WriteLine(worker);
            }

            return true;
        }
    }
}
