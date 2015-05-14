using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections;

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
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = tcpPort;
            props["timeout"] = 6000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkerServices), remoteObjectName, WellKnownObjectMode.Singleton);

            try {

                IWorker newWorkerObj = (IWorker)Activator.GetObject(typeof(IWorker), serviceUri.ToString());

                // Register new worker in itself passing the ID and the URI
                newWorkerObj.RegisterOwnWorker(id, serviceUri.ToString());

            } catch (SocketException e) {

                System.Console.WriteLine("[WORKER_ERROR1:MAIN] Could not locate server");
                System.Console.WriteLine(e.StackTrace);

            } catch (Exception e) {

                System.Console.WriteLine("[WORKER_ERROR2:MAIN] Could not locate server");
                System.Console.WriteLine(e.StackTrace);
            }

            // If outer jobtracker exists
            if (args.Length >= 3) {

                try {

                    String jobTrackerUri = args[2];
                    IWorker jobTrackerObj = (IWorker)Activator.GetObject(typeof(IWorker), jobTrackerUri);
                    // Broadcast to the network the new worker
                    jobTrackerObj.BroadcastNewWorker(id, serviceUri.ToString());

                } catch (SocketException e) {

                    System.Console.WriteLine("[WORKER_ERROR3:MAIN] Could not locate server");
                    System.Console.WriteLine(e.StackTrace);

                } catch (Exception e) {

                    System.Console.WriteLine("WORKER_ERROR4:MAIN] Could not locate server");
                    System.Console.WriteLine(e.StackTrace);

                }
            }

            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine();

        }

    }

    internal class WorkerServices : MarshalByRefObject, IWorker {

        // ====================================================
        // Network Related Variables
        // ====================================================

        /// <summary>
        /// Url of this service
        /// </summary>
        private string ownUrl;

        /// <summary>
        /// Worker id
        /// </summary>
        private int ownId;

        // ====================================================
        // Job Tracker Variables
        // ====================================================

        /// <summary>
        /// True if the job was concluded
        /// </summary>
        private bool jobDone = false;

        /// <summary>
        /// Workers HashSet with the workerId and its url
        /// </summary>
        /// 
        private HashSet<KeyValuePair<int, string>> workersIDUrl;

        /// <summary>
        /// Mutex to guarantee the mutual exclusion
        /// </summary>
        private Mutex workersIDUrlMutex = new Mutex();

        /// <summary>
        /// HashSet that holds the WorkStatus for each split
        /// </summary>
        private HashSet<WorkStatus> jobsAssigment;

        /// <summary>
        /// Mutex to guarantee the mutual exclusion
        /// </summary>
        private Mutex jobsAssigmentMutex = new Mutex();

        /// <summary>
        /// Queue with the available workers
        /// </summary>
        private ArrayList availableWorkerToJob;

        /// <summary>
        /// Mutex to guarantee the mutual exclusion
        /// </summary>
        private Mutex availableWorkerToJobMutex = new Mutex();

        /// <summary>
        /// Semaphore to control the available workers
        /// </summary>
        private Semaphore queueWorkersSemaphore;

        // ====================================================
        // Worker Variables
        // ====================================================

        /// <summary>
        /// Job Tracker for the current job. If there isn't a job running is null 
        /// </summary>
        private string currentJobTracker = "";

        /// <summary>
        /// The status of the split of this worker
        /// </summary>
        private WorkStatus myWorkerStatus = new WorkStatus();

        /// <summary>
        /// The delay of the thread that runs the job, in seconds
        /// </summary>
        private int threadDelay = 0;

        /// <summary>
        /// Mutex to guarantee the mutual exclusion
        /// </summary>
        private Mutex delayMutex = new Mutex();

        /// <summary>
        /// Is the worker frozen
        /// </summary>
        private bool isFrozenW = false;

        /// <summary>
        /// Object to synchronize isfrozenW
        /// </summary>
        private Object frozenWLock = new Object();

        /// <summary>
        /// Is the jobtracker frozen
        /// </summary>
        private bool isFrozenC = false;

        /// <summary>
        /// Object to synchronize isfrozenC
        /// </summary>
        private Object frozenCLock = new Object();

        private string className;

        private byte[] dllCode;

        private string clientUrl;

        // ====================================================
        // Constructors
        // ====================================================

        public WorkerServices() {

            workersIDUrl = new HashSet<KeyValuePair<int, string>>();

        }

        // ====================================================
        // Job Tracker Functions
        // ====================================================

        // Job Tracking

        /// <summary>
        /// Called on jobtracker and broadcast to the network the new job
        /// </summary>
        /// <param name="clientUrl"></param>
        /// <param name="inputSize"></param>
        /// <param name="className"></param>
        /// <param name="dllCode"></param>
        /// <param name="NumberOfSplits"></param>
        /// <returns></returns>
        public bool RequestJob(string clientUrl, long inputSize, string className, byte[] dllCode, int NumberOfSplits) {

            //This workers will function as a jobtracker!

            //Init variables to new job

            jobDone = false;

            jobsAssigment = new HashSet<WorkStatus>();

            //Split the inputFile between the works
            long splitSize = inputSize / NumberOfSplits;
            long remainder = inputSize % NumberOfSplits;


            for (long i = 0; i < inputSize; i += splitSize) {

                long beginIndex = i;
                long endIndex = beginIndex + splitSize - 1;

                //Is last split?
                if (beginIndex + splitSize + remainder >= inputSize) {
                    endIndex += remainder + 1;
                    i += remainder + 1;
                }

                //Add this job to the HashSet
                jobsAssigmentMutex.WaitOne();
                jobsAssigment.Add(new WorkStatus(beginIndex, endIndex));
                jobsAssigmentMutex.ReleaseMutex();
  
            }

            // Create Queue with workers
            availableWorkerToJob = new ArrayList();

            foreach (KeyValuePair<int,string> worker in workersIDUrl)
                availableWorkerToJob.Add(worker.Value);

            queueWorkersSemaphore = new Semaphore(workersIDUrl.Count(), workersIDUrl.Count());

            //Launch thread to isAlive
            Thread isAliveThread = new Thread(() => IsAliveAsync());
            isAliveThread.Start();
            

            //Launch Thread to distribute works
            Thread distributeWorkAsyncThread = new Thread(() => DistributeWorkAsync(className, dllCode, clientUrl));
            distributeWorkAsyncThread.Start();

            return true;
        }

        /// <summary>
        /// Call if one worker is available 
        /// </summary>
        /// <param name="workerUrl">worker url</param>
        /// <returns></returns>
        public bool FinishProcessing(string workerUrl, WorkStatus workerStatus) {

            TestFrozenC();
            jobsAssigmentMutex.WaitOne();

            foreach (WorkStatus iWorkStatus in jobsAssigment) {

                if (iWorkStatus.getWorkerId() == workerStatus.getWorkerId() && iWorkStatus.getBeginIndex() == workerStatus.getBeginIndex()) {

                    iWorkStatus.setLastModification(workerStatus.getLastModification());
                    iWorkStatus.setNumberLinesComputed(workerStatus.getNumberLinesComputed());
                    iWorkStatus.setTotalNumberOfLines(workerStatus.getTotalNumberLines());
                    iWorkStatus.setWorkerId(workerStatus.getWorkerId());

                    //Console.WriteLine("FINISH " + workerStatus.getWorkerId());
                    break;
                }

                
            }
            
            jobsAssigmentMutex.ReleaseMutex();

            availableWorkerToJobMutex.WaitOne();
            bool exists = availableWorkerToJob.Contains(workerUrl);
            availableWorkerToJobMutex.ReleaseMutex();

            if (!exists) {

                availableWorkerToJobMutex.WaitOne();
                availableWorkerToJob.Add(workerUrl);
                availableWorkerToJobMutex.ReleaseMutex();

                queueWorkersSemaphore.Release(1);

            }

            

            return true;
        }

        /// <summary>
        /// Test if the split of beginSplit is assigned to workerId
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="beginSplit"></param>
        /// <returns>Returns true if the split is not done and assigned to that workerId</returns>
        public bool isSplitValid(int workerId, long beginSplit) {

            TestFrozenC();

            if (currentJobTracker != ownUrl) //i'm not the jobTracker anymore
                return false;

            bool isValid = false;
            
            jobsAssigmentMutex.WaitOne();

            //See if that job is assigned to this worker
            foreach (WorkStatus workStatus in jobsAssigment) {

                //Console.WriteLine(workStatus.getWorkerId() + " == " + workerId + " && " + workStatus.getBeginIndex() + " == " + beginSplit + " && " + !workStatus.isWorkCompleted());
                if (workStatus.getWorkerId() == workerId && workStatus.getBeginIndex() == beginSplit && !workStatus.isWorkCompleted()) {
                    
                    isValid = true;
                    break;

                }

            }

            jobsAssigmentMutex.ReleaseMutex();

                        return isValid;
        }
        // Register Nodes

        /// <summary>
        /// Broadcast the new worker to the whole network 
        /// </summary>
        /// <param name="url">new worker url</param>
        /// <returns></returns>
        public bool BroadcastNewWorker(int id, string url) {

            //Add to own workers list
            workersIDUrlMutex.WaitOne();
            workersIDUrl.Add(new KeyValuePair<int,string>(id,url));
            workersIDUrlMutex.ReleaseMutex();

            //Send to for each element of workerUrl the new worker Url
            workersIDUrlMutex.WaitOne();
            foreach (KeyValuePair<int,string> workerPair in workersIDUrl) {

                string worker = workerPair.Value;

                if (!worker.Equals(url) && !worker.Equals(ownUrl)) {

                    IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), worker);

                    try {
                        //Create list with the new worker url
                        HashSet<KeyValuePair<int, string>> workers = new HashSet<KeyValuePair<int, string>>();
                        workers.Add(new KeyValuePair<int, string>(id,url));

                        //Send to an already existing worker the new one
                        workerObj.RegisterNewWorkers(workers);

                    } catch (SocketException) {

                        System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR1] Could not locate server");
                        return false;

                    } catch (Exception e) {

                        System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR2] " + e.StackTrace);
                        return false;
                    }

                }

            }
            workersIDUrlMutex.ReleaseMutex();

            //Send to the new worker a complete list of the workers urls
            IWorker newWorkerObj = (IWorker)Activator.GetObject(typeof(IWorker), url);

            try {

                //Create list with the new worker url
                HashSet<KeyValuePair<int, string>> workers = new HashSet<KeyValuePair<int, string>>();

                workersIDUrlMutex.WaitOne();
                foreach (KeyValuePair<int, string> workerPair in workersIDUrl) {
                    workers.Add(workerPair);
                }
                workersIDUrlMutex.ReleaseMutex();

                workers.Remove(new KeyValuePair<int,string>(id,url));

                //Send to an already existing worker the new one
                newWorkerObj.RegisterNewWorkers(workers);

            } catch (SocketException) {

                System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR3] Could not locate server");
                return false;

            } catch (Exception e) {

                System.Console.WriteLine("[BROADCAST_NEW_WORKER_ERR4]" + e.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Test if the node is a job tracker and if it is add the caller to the workers list
        /// </summary>
        /// <returns></returns>
        public bool IsJobTrackerAlive(int workerId, string workerUrl, bool working) {

            TestFrozenC();

            if (currentJobTracker != ownUrl) //i'm not the jobTracker anymore
                return false;

            //Is the worker in our list
            workersIDUrlMutex.WaitOne();
            bool exists = workersIDUrl.Contains(new KeyValuePair<int,string>(workerId, workerUrl));
            workersIDUrlMutex.ReleaseMutex();

            if (!exists) {

                //Add to the network
                workersIDUrlMutex.WaitOne();
                workersIDUrl.Add(new KeyValuePair<int, string>(workerId, workerUrl));
                workersIDUrlMutex.ReleaseMutex();


                if (!working) {

                    availableWorkerToJobMutex.WaitOne();
                    exists = availableWorkerToJob.Contains(workerUrl);
                    availableWorkerToJobMutex.ReleaseMutex();

                    if (!exists) {

                        //Add to current job available workers
                        availableWorkerToJobMutex.WaitOne();
                        availableWorkerToJob.Add(workerUrl);
                        availableWorkerToJobMutex.ReleaseMutex();

                        queueWorkersSemaphore.Release(1);

                    }

                }
            }
            return true;
        }


        public bool IsNodeInNetwork(KeyValuePair<int, string> node) {

            bool isInNetwork = false;

            workersIDUrlMutex.WaitOne();

            foreach(KeyValuePair<int, string> worker in workersIDUrl){
                    
                if(worker.Value == node.Value && worker.Key == node.Key)
                    isInNetwork = true;
            }

            workersIDUrlMutex.ReleaseMutex();
            return isInNetwork;

        }
        // ====================================================
        // Worker Functions
        // ====================================================

        // Run jobs
        
        /// <summary>
        /// Starts new thread to run the job sent
        /// </summary>
        /// <param name="className"></param>
        /// <param name="dllCode"></param>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="clientUrl"></param>
        /// <param name="jobTrackerUrl"></param>
        /// <returns></returns>
        public bool RunJob(string className, byte[] dllCode, long beginIndex, long endIndex, string clientUrl, string jobTrackerUrl) {

            this.className = className;

            this.dllCode = dllCode;

            this.clientUrl = clientUrl;

            myWorkerStatus.setSplitIndexes(beginIndex, endIndex);
            myWorkerStatus.setWorkerId(ownId);
            myWorkerStatus.setNumberLinesComputed(0);

            currentJobTracker = jobTrackerUrl;

            Thread thread = new Thread(() => RunAsyncJob(className, dllCode, beginIndex, endIndex, clientUrl));
            thread.Start();

            return false;

        }

        /// <summary>
        /// Async method to run job
        /// </summary>
        /// <param name="className"></param>
        /// <param name="dllCode"></param>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="clientUrl"></param>
        /// <param name="jobTrackerUrl"></param>
        public void RunAsyncJob(string className, byte[] dllCode, long beginIndex, long endIndex, string clientUrl) {

            // Get Input Split from client
            myWorkerStatus.changeStatus(WorkStatus.Status.GettingInput);
            IClient clientObj = (IClient)Activator.GetObject(typeof(IClient), clientUrl);

            TestFrozenW(); //Test if it is frozen
            string input = clientObj.getInputSplit(ownId, beginIndex, endIndex);

            // Fetch dll 
            myWorkerStatus.changeStatus(WorkStatus.Status.FetchingInput);

            Assembly assembly = Assembly.Load(dllCode);

            foreach (Type type in assembly.GetTypes()) {

                if (type.IsClass == true) {

                    if (type.FullName.EndsWith("." + className)) {

                        try {

                            //Update Status
                            myWorkerStatus.setLastModification(DateTime.Now);

                            myWorkerStatus.setTotalNumberOfLines(input.Count(x => x == '\n') + 1);
                            myWorkerStatus.setNumberLinesComputed(0);

                            //Start invoking the methods 

                            myWorkerStatus.changeStatus(WorkStatus.Status.Computing);

                            object ClassObj = Activator.CreateInstance(type);

                            StringBuilder sb = new StringBuilder();

                            using (StringReader sr = new StringReader(input)) {

                                String inputLine;

                                while ((inputLine = sr.ReadLine()) != null) {

                                    delayMutex.WaitOne();

                                    if (this.threadDelay > 0) {
                                        Thread.Sleep(this.threadDelay * 1000);
                                        this.threadDelay = 0;
                                    }
                                    
                                    delayMutex.ReleaseMutex();

                                    // Dynamically Invoke the method
                                    object[] args = new object[] { inputLine };

                                    //FREEZEW - Not Able to Compute
                                    TestFrozenW();
                                    object resultObject = type.InvokeMember("Map", BindingFlags.Default | BindingFlags.InvokeMethod, null, ClassObj, args);


                                    myWorkerStatus.setNumberLinesComputed(myWorkerStatus.getNumberLinesComputed() + 1);

                                    foreach (KeyValuePair<string, string> resultPair in (IList<KeyValuePair<string, string>>)resultObject) {
                                        sb.Append(resultPair.Key);
                                        sb.Append(":");
                                        sb.Append(resultPair.Value);
                                        sb.Append(Environment.NewLine);
                                    }

                                }

                            }

                            //Test if the work is still valid
                            IWorker jobTracker = (IWorker)Activator.GetObject(typeof(IWorker), currentJobTracker);

                            //FREEZEW - Not Able to Communicate with outside
                            TestFrozenW();
                            bool splitValid = jobTracker.isSplitValid(ownId, beginIndex);


                            if (splitValid) {

                                myWorkerStatus.changeStatus(WorkStatus.Status.SendingOutput);

                                //FREEZEW - Not Able to Communicate with outside
                                TestFrozenW();

                                // Send processed split to the client with my Id and the Result
                                clientObj.sendProcessedSplit(ownId, sb.ToString());

                            }

                            //FREEZEW - Not Able to Communicate with outside
                            TestFrozenW();
                            jobTracker.FinishProcessing(ownUrl, myWorkerStatus);
                            
                            myWorkerStatus.changeStatus(WorkStatus.Status.Idle);

                        } catch (Exception e) {

                            System.Console.WriteLine("IMPOSSIBLE TO CONNECT TO JOBTRACKER ON: " + currentJobTracker);
                            /*System.Console.WriteLine("[WORKER_SERVICES_ERROR1:RUN_ASYNC_JOB] Could not invoke method.");
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.StackTrace);
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.GetType());
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.Message);
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.InnerException.Message);
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.InnerException.StackTrace);*/

                        }


                    }

                }

            }

        }
        
        // Register workers

        /// <summary>
        /// Register own worker
        /// </summary>
        /// <param name="id">worker id</param>
        /// <param name="url">worker url</param>
        /// <returns></returns>
        public bool RegisterOwnWorker(int id, string url) {

            ownUrl = url;
            ownId = id;

            workersIDUrl.Add(new KeyValuePair<int, string>(id, url));

            //Create Thread that tells either the network or the jobTracker that the node is alive
            Thread WorkerIsAliveThread = new Thread(() => WorkerIsAliveAsync());
            WorkerIsAliveThread.Start();

            return true;
        }

        /// <summary>
        /// Register list of workers
        /// </summary>
        /// <param name="workerServiceUrl">list fo workers</param>
        /// <returns></returns>
        public bool RegisterNewWorkers(HashSet<KeyValuePair<int, string>> workerServiceUrl) {



            workersIDUrlMutex.WaitOne();
            foreach (KeyValuePair<int, string> workerPair in workerServiceUrl) {

                bool isDuplicate = false;

                foreach(KeyValuePair<int,string> currentWorkerPair in workersIDUrl){

                    if(workerPair.Value == currentWorkerPair.Value && workerPair.Key == workerPair.Key)
                        isDuplicate = true;

                }

                if(!isDuplicate)
                    workersIDUrl.Add(workerPair);
    
            }
            workersIDUrlMutex.ReleaseMutex();

            return true;

        }

        /// <summary>
        /// Sends the current WorkStatus to the jobtracker
        /// </summary>
        /// <param name="currentViewWorkers">current view of the workers</param>
        /// <param name="currentJobAssigment">current job assigment</param>
        /// <returns>This worker work status</returns>
        public WorkStatus IsAlive(HashSet<KeyValuePair<int, string>> currentViewWorkers, HashSet<WorkStatus> currentJobAssigment) {

            TestFrozenW();

            //Update current View Workers
            workersIDUrlMutex.WaitOne();
            this.workersIDUrl = currentViewWorkers;
            workersIDUrlMutex.ReleaseMutex();

            //Update current View Job Assigment
            jobsAssigmentMutex.WaitOne();
            this.jobsAssigment = currentJobAssigment;
            jobsAssigmentMutex.ReleaseMutex();

            //Return current workStatus
            return (WorkStatus)myWorkerStatus.Clone();

        }
        
        // Command related methods

        /// <summary>
        /// Prints the status of the worker
        /// </summary>
        /// <returns></returns>
        public bool PrintStatus() {

            Console.WriteLine("STATUS: ");
            Console.WriteLine("My Url: " + ownUrl);

            Console.WriteLine("Connections: ");

            workersIDUrlMutex.WaitOne();
            foreach (KeyValuePair<int,string> workerPair in workersIDUrl) {
                Console.WriteLine("[ID: " + workerPair.Key + " , URL: " + workerPair.Value + "]");
            }
            workersIDUrlMutex.ReleaseMutex();

            Console.WriteLine("Current task:");
            Console.WriteLine(myWorkerStatus.getStatus());

            if (myWorkerStatus.getStatus() == WorkStatus.Status.Computing) {
                Console.WriteLine("Done:");
                Console.WriteLine(myWorkerStatus.getNumberLinesComputed() + "/" + myWorkerStatus.getTotalNumberLines());
            }


            return true;
        }

        /// <summary>
        /// Slow the worker by delay seconds
        /// </summary>
        /// <param name="delay">number of seconds to delay worker</param>
        /// <returns></returns>
        public bool SlowWorker(int delay) {

            delayMutex.WaitOne();
            threadDelay += delay;
            delayMutex.ReleaseMutex();

            return true;

        }

        /// <summary>
        /// Freezes the Worker computation and communication
        /// </summary>
        /// <returns></returns>
        public bool FreezeW() {

            lock (frozenWLock) {
                isFrozenW = true;
            }

            return true;
            
        }

        /// <summary>
        /// Unfreezes the worker computation and communication
        /// </summary>
        /// <returns></returns>
        public bool UnfreezeW() {

            lock (frozenWLock) {
                isFrozenW = false;
                Monitor.PulseAll(frozenWLock);
            }
            return true;
        }

        /// <summary>
        /// Blocks if worker is FrozenW
        /// </summary>
        private void TestFrozenW() {

            lock (frozenWLock) {
                while (isFrozenW)
                    Monitor.Wait(frozenWLock);
            }

        }
        
        public bool FreezeC(){

            lock (frozenCLock) {
                isFrozenC = true;
            }

            return true;
        }   

        public bool UnfreezeC() {

            lock (frozenCLock) {
                isFrozenC = false;
                Monitor.PulseAll(frozenCLock);
            }
            return true;
        }

        private void TestFrozenC() {

            lock (frozenCLock) {
                while (isFrozenC)
                    Monitor.Wait(frozenCLock);
            }

        }

        // =============================================
        // Threads Section
        // =============================================

        /// <summary>
        /// Async method to keep tracking of alive workers
        /// </summary>
        public void IsAliveAsync() {

            while (!jobDone) {

                TestFrozenC();

                //Is alive interval
                Thread.Sleep(2000);

                //Create a copy of the workersUrl
                workersIDUrlMutex.WaitOne();
                HashSet<KeyValuePair<int, string>> clonedWorkersUrl = new HashSet<KeyValuePair<int, string>>(workersIDUrl);
                workersIDUrlMutex.ReleaseMutex();

                jobsAssigmentMutex.WaitOne();
                HashSet<WorkStatus> clonedJobAssigments = new HashSet<WorkStatus>(jobsAssigment);
                jobsAssigmentMutex.ReleaseMutex();

                foreach (KeyValuePair<int, string> worker in clonedWorkersUrl) {
                        
                    //Get the worker
                    IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), worker.Value);

                    try {
                        
                        //Retrive the workStatus of the worker
                        WorkStatus workerStatus = workerObj.IsAlive(clonedWorkersUrl, clonedJobAssigments);


                        //Update the workStatus of that worker

                        jobsAssigmentMutex.WaitOne();
                        foreach (WorkStatus iWorkStatus in jobsAssigment) {

                            if (iWorkStatus.getWorkerId() == workerStatus.getWorkerId() && iWorkStatus.getBeginIndex() == workerStatus.getBeginIndex()) {

                                iWorkStatus.setLastModification(workerStatus.getLastModification());
                                iWorkStatus.setNumberLinesComputed(workerStatus.getNumberLinesComputed());
                                iWorkStatus.setTotalNumberOfLines(workerStatus.getTotalNumberLines());
                                iWorkStatus.setWorkerId(workerStatus.getWorkerId());

                                //Console.WriteLine("isalive"  + workerStatus.getWorkerId());

                                break;
                            }

                        }
                        jobsAssigmentMutex.ReleaseMutex();
    
                        //TODO: test if the worker is too slow
                        




                    } catch (Exception) { //The worker didn't anwser

                        Console.WriteLine("***************" + worker.Value + "dead");

                        //Remove the worker from the workersList
                        workersIDUrlMutex.WaitOne();
                        workersIDUrl.Remove(worker);
                        workersIDUrlMutex.ReleaseMutex();

                        //Removes all the works not done and assigned to it
                        jobsAssigmentMutex.WaitOne();
                        foreach (WorkStatus iWorkStatus in jobsAssigment) {

                            if (iWorkStatus.getWorkerId() == worker.Key && !iWorkStatus.isWorkCompleted()) {

                                iWorkStatus.removeWorker();

                            }

                        }
                        jobsAssigmentMutex.ReleaseMutex();


                        //Remove from available worker
                        availableWorkerToJobMutex.WaitOne();
                        availableWorkerToJob.Remove(worker.Value);
                        availableWorkerToJobMutex.ReleaseMutex();
                    
                    }

                }
                   
            }
    
        }

        /// <summary>
        /// Distributes the split by the workers as new workers are available
        /// </summary>
        /// <param name="className">ClassName to run</param>
        /// <param name="dllCode">Class dll to run</param>
        /// <param name="clientUrl">Client Url to communicate answer</param>
        public void DistributeWorkAsync(string className, byte[] dllCode, string clientUrl) {

            while (!jobDone) {

                

                TestFrozenC();

                //Get first job available

                jobsAssigmentMutex.WaitOne();


                //All jobs concluded
                bool isCompleted = true;

                foreach (WorkStatus iWorkStatus in jobsAssigment) {

                    if (!iWorkStatus.isWorkCompleted()) {

                        isCompleted = false;
                        break;

                    }
                }

                if (isCompleted) {
                    jobsAssigmentMutex.ReleaseMutex();
                    jobDone = true;
                    break;
                }


                //Still work to do
                WorkStatus freeWorkStatus = null;

                foreach (WorkStatus iWorkStatus in jobsAssigment) {

                    //Console.WriteLine(iWorkStatus.isWorkerAssigned() + " => " + iWorkStatus.getBeginIndex());

                    if (!iWorkStatus.isWorkerAssigned()) {

                        freeWorkStatus = iWorkStatus;
                        break;

                    }
                }

                //No more works to be done
                if (freeWorkStatus == null) {
                    jobsAssigmentMutex.ReleaseMutex();
                    continue;
                }

                //temporary assignment
                freeWorkStatus.setWorkerId(-2);
                long freeWorkStatusBeginIndex = freeWorkStatus.getBeginIndex();
                jobsAssigmentMutex.ReleaseMutex();

                //Get first free worker
                queueWorkersSemaphore.WaitOne();

                //Send the job to worker
                availableWorkerToJobMutex.WaitOne();

                String workerUrl = (string)availableWorkerToJob[0];
                availableWorkerToJob.RemoveAt(0);
                availableWorkerToJobMutex.ReleaseMutex();

                if (workerUrl == null)
                    throw new ArgumentNullException();



                //System.Console.WriteLine("[DISTRIBUTE_WORK] Sending job to " + workerUrl + " with split [" + freeWorkStatus.getBeginIndex() + ", " + freeWorkStatus.getEndIndex() + "] ... ");

                IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), workerUrl);


                try {

                    //Get id from url
                    int workerID = 0;

                    //Get id by url
                    workersIDUrlMutex.WaitOne();
                    foreach (KeyValuePair<int, string> workerPair in workersIDUrl) {

                        if (workerPair.Value == workerUrl) {

                            workerID = workerPair.Key;

                            break;
                        }

                    }
                    workersIDUrlMutex.ReleaseMutex();

                    //Assign that job to this worker
                    jobsAssigmentMutex.WaitOne();

                    foreach (WorkStatus ws in jobsAssigment) {
                        
                        if (freeWorkStatusBeginIndex == ws.getBeginIndex()) {
                            freeWorkStatus = ws;
                        }

                    }

                    freeWorkStatus.setWorkerId(workerID);
                    workerObj.RunJob(className, dllCode, freeWorkStatus.getBeginIndex(), freeWorkStatus.getEndIndex(), clientUrl, ownUrl);




                    jobsAssigmentMutex.ReleaseMutex();
                    



                } catch (Exception e) {


                    System.Console.WriteLine("[ERROR_DISTRIBUTE_WORK1]  Cant send work to " + workerUrl);
                    jobsAssigmentMutex.WaitOne();
                    freeWorkStatus.removeWorker();
                    jobsAssigmentMutex.ReleaseMutex();

                    availableWorkerToJobMutex.WaitOne();
                    availableWorkerToJob.Add(workerUrl);
                    availableWorkerToJobMutex.ReleaseMutex();

                    queueWorkersSemaphore.Release(1);

                }

            }
        }

        /// <summary>
        /// Alerts the network that this node is alive
        /// </summary>
        public void WorkerIsAliveAsync() {

            while (true) {

                TestFrozenW();

                Thread.Sleep(2000);

                //Can we connect to a jobTracker
                bool jobTrackerAvailable = false;

                //Is Worker Working
                if (myWorkerStatus.getStatus() != WorkStatus.Status.Idle) {

                    //Ask the jobTracker if he is alive
                    IWorker jobTrackerObject = (IWorker)Activator.GetObject(typeof(IWorker), currentJobTracker);

                    try {

                        bool isJobTracker = jobTrackerObject.IsJobTrackerAlive(ownId, ownUrl, myWorkerStatus.getStatus() != WorkStatus.Status.Idle);

                        jobTrackerAvailable = isJobTracker; //Worker connected but his he the jobTracker

                    } catch (Exception) { //Cannot connect to job Tracker - 1st try

                        //Try again
                        try {

                            bool isJobTracker = jobTrackerObject.IsJobTrackerAlive(ownId, ownUrl, myWorkerStatus.getStatus() != WorkStatus.Status.Idle);

                            jobTrackerAvailable = isJobTracker; //Worker connected but his he the jobTracker

                        } catch (Exception) { //Cannot connect to job Tracker 2nd try

                            Console.WriteLine("Cannot connect to primary jobTracker:" + currentJobTracker);

                        }

                    }


                    if (!jobTrackerAvailable) { //Job Tracker not available

                        Console.WriteLine("Cannot connect to primary jobtracker on: " + currentJobTracker + "! Testing second jobtracker");

                        int id = int.MaxValue;
                        string url = "";

                        //Get the first non-jobtracker on the last view
                        workersIDUrlMutex.WaitOne();
                        foreach (KeyValuePair<int, string> workerPair in workersIDUrl) {

                            if (workerPair.Value != currentJobTracker && workerPair.Key < id) {

                                id = workerPair.Key;
                                url = workerPair.Value;
                            }

                        }
                        workersIDUrlMutex.ReleaseMutex();

                        Console.WriteLine(">>>" + url);

                        if (id == ownId && url == ownUrl) {

                            Console.WriteLine("I'm new jobtracker");

                            // Create Queue with workers
                            availableWorkerToJob = new ArrayList();

                            foreach (KeyValuePair<int, string> worker in workersIDUrl)
                                availableWorkerToJob.Add(worker.Value);

                            queueWorkersSemaphore = new Semaphore(workersIDUrl.Count(), workersIDUrl.Count());

                            currentJobTracker = url;

                            jobsAssigmentMutex.WaitOne();
                            foreach (WorkStatus ws in jobsAssigment) {

                                if (ws.isWorkerAssigned() && !ws.isWorkCompleted()) {
                                    ws.removeWorker();
                                }

                            }
                            jobsAssigmentMutex.ReleaseMutex();

                            //Launch thread to isAlive
                            Thread isAliveThread = new Thread(() => IsAliveAsync());
                            isAliveThread.Start();


                            //Launch Thread to distribute works
                            Thread distributeWorkAsyncThread = new Thread(() => DistributeWorkAsync(className, dllCode, clientUrl));
                            distributeWorkAsyncThread.Start();

                        } else {

                            Console.WriteLine("New jobtracker on: " + url);

                            currentJobTracker = url;
                            jobTrackerAvailable = true;
                            /*
                            //Ask the secondJobTracker if he is alive
                            IWorker secondJobTrackerObject = (IWorker)Activator.GetObject(typeof(IWorker), url);

                            try {

                                bool isSecondJobTracker = secondJobTrackerObject.IsJobTrackerAlive(ownId, ownUrl, myWorkerStatus.getStatus() != WorkStatus.Status.Idle);

                                if (isSecondJobTracker) {//Second jobTracker is alive but is he jobtracker?

                                    currentJobTracker = url;
                                    jobTrackerAvailable = true;

                                }

                            } catch (Exception) { //Cannot connect to second job Tracker 2nd try

                                //Nothing to do
                            }*/
                        }
                    }
                }

                if (!jobTrackerAvailable) { //When none of the jobTrackers are alive  


                    foreach(KeyValuePair<int,string> node in workersIDUrl){

                        try{
                        
                            IWorker nodeObj = (IWorker) Activator.GetObject(typeof(IWorker), node.Value);

                            //Am I in the network?
                            if(!nodeObj.IsNodeInNetwork(node)){

                                nodeObj.BroadcastNewWorker(ownId,ownUrl);

                            }

                            //When it is in the network
                            break;

                        }catch (Exception e){

                            //Can't connect to this one, try another one
                             continue;
                        }

                    } // if no worker connects it means that the network doesn't exist

                }
            }
 




            

            


        }
    }
}