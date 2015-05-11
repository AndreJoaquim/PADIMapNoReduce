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
        private Queue<string> availableWorkerToJob;

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
            availableWorkerToJob = new Queue<string>();
            availableWorkerToJob.Enqueue(ownUrl);

            foreach (KeyValuePair<int,string> worker in workersIDUrl)
                availableWorkerToJob.Enqueue(worker.Value);

            queueWorkersSemaphore = new Semaphore(workersIDUrl.Count + 1, workersIDUrl.Count + 1);
            
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
        public bool FinishProcessing(string workerUrl) {

            availableWorkerToJobMutex.WaitOne();
            availableWorkerToJob.Enqueue(workerUrl);
            availableWorkerToJobMutex.ReleaseMutex();

            queueWorkersSemaphore.Release(1);

            return true;
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

                if (!worker.Equals(url)) {

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

                workers.Add( new KeyValuePair<int,String>(ownId, ownUrl));

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

            myWorkerStatus.setWorkerId(ownId);
            myWorkerStatus.setNumberLinesComputed(0);

            Thread thread = new Thread(() => RunAsyncJob(className, dllCode, beginIndex, endIndex, clientUrl, jobTrackerUrl));
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
        public void RunAsyncJob(string className, byte[] dllCode, long beginIndex, long endIndex, string clientUrl, string jobTrackerUrl) {

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

                            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

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
                                        result.Add(resultPair);
                                    }

                                }

                            }

                            myWorkerStatus.changeStatus(WorkStatus.Status.SendingOutput);

                            // Send processed split to the client with my Id and the Result

                            //FREEZEW - Not Able to Communicate with outside
                            TestFrozenW();
                            clientObj.sendProcessedSplit(ownId, result);


                            IWorker jobTracker = (IWorker)Activator.GetObject(typeof(IWorker), jobTrackerUrl);

                            //FREEZEW - Not Able to Communicate with outside
                            TestFrozenW();
                            jobTracker.FinishProcessing(ownUrl);
                            
                            myWorkerStatus.changeStatus(WorkStatus.Status.Idle);

                        } catch (Exception e) {

                            System.Console.WriteLine("[WORKER_SERVICES_ERROR1:RUN_ASYNC_JOB] Could not invoke method.");
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.StackTrace);
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.GetType());
                            System.Console.WriteLine("=========================");
                            System.Console.WriteLine(e.Message);
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

            //Update current View Workers

            //Update current View Job Assigment

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
            Console.WriteLine("Done:");
            Console.WriteLine(myWorkerStatus.getNumberLinesComputed() + "/" + myWorkerStatus.getTotalNumberLines());

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
                Monitor.PulseAll(frozenWLock);
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

                //Create a copy of the workersUrl

                workersIDUrlMutex.WaitOne();
                HashSet<KeyValuePair<int, string>> clonedWorkersUrl = new HashSet<KeyValuePair<int, string>>(workersIDUrl);
                workersIDUrlMutex.ReleaseMutex();

                foreach (KeyValuePair<int, string> worker in clonedWorkersUrl) {
                        
                    //Get the worker
                    IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), worker.Value);

                    try {

                        //System.Console.WriteLine("[ISALIVE] is " + worker.Value + " alive ?");

                        //Retrive the workStatus of the worker
                        workersIDUrlMutex.WaitOne();
                        jobsAssigmentMutex.WaitOne();
                        WorkStatus workerStatus = workerObj.IsAlive(new HashSet<KeyValuePair<int,string>>(workersIDUrl), new HashSet<WorkStatus>(jobsAssigment));
                        jobsAssigmentMutex.ReleaseMutex();
                        workersIDUrlMutex.ReleaseMutex();

                        //System.Console.WriteLine("[ISALIVE] " + worker.Value + " is alive ");

                        //Update the workStatus of that worker
                        jobsAssigmentMutex.WaitOne();
                        foreach (WorkStatus iWorkStatus in jobsAssigment) {

                            if (iWorkStatus.getWorkerId() == workerStatus.getWorkerId() && iWorkStatus.getBeginIndex() == workerStatus.getBeginIndex()) {

                                iWorkStatus.setLastModification(workerStatus.getLastModification());
                                iWorkStatus.setNumberLinesComputed(workerStatus.getNumberLinesComputed());
                                iWorkStatus.setTotalNumberOfLines(workerStatus.getTotalNumberLines());
                                iWorkStatus.setWorkerId(workerStatus.getWorkerId());
                                    
                                break;
                            }

                        }
                        jobsAssigmentMutex.ReleaseMutex();
    
                        //TODO: test if the worker is too slow

                        



                    } catch (SocketException) { //The worker didn't anwser

                        //System.Console.WriteLine("[ISALIVE1] Couldnt communicate to worker: " + worker.Value );
                            
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

                    } catch (Exception e) {
                        System.Console.WriteLine("[ISALIVE2] " + e.GetType());
                        System.Console.WriteLine("[ISALIVE2] " + e.Message);
                        System.Console.WriteLine("[ISALIVE2] " + e.StackTrace);
                        Console.ReadLine();    
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


                //Get first free worker
                queueWorkersSemaphore.WaitOne();

                //Send the job to worker
                availableWorkerToJobMutex.WaitOne();
                String workerUrl = availableWorkerToJob.Dequeue();
                availableWorkerToJobMutex.ReleaseMutex();
                if (workerUrl == null)
                    throw new ArgumentNullException();

                //Get first job available
                jobsAssigmentMutex.WaitOne();

                WorkStatus freeWorkStatus = null;

                foreach (WorkStatus iWorkStatus in jobsAssigment) {

                    //Console.WriteLine(iWorkStatus.isWorkerAssigned() + " => " + iWorkStatus.getBeginIndex());

                    if (!iWorkStatus.isWorkerAssigned()) {

                        freeWorkStatus = iWorkStatus;
                        break;

                    }
                }


                //No more works to be done
                //TODO: test if all the works are done
                if (freeWorkStatus == null) {
                    jobsAssigmentMutex.ReleaseMutex();
                    jobDone = true;
                    break;
                }
                
                //System.Console.WriteLine("[DISTRIBUTE_WORK] Sending job to " + workerUrl + " with split [" + freeWorkStatus.getBeginIndex() + ", " + freeWorkStatus.getEndIndex() + "] ... ");

                IWorker workerObj = (IWorker)Activator.GetObject(typeof(IWorker), workerUrl);

                try {


                    workerObj.RunJob(className, dllCode, freeWorkStatus.getBeginIndex(), freeWorkStatus.getEndIndex(), clientUrl, ownUrl);

                    //Get id from url
                    int workerID = 0;

                    //Is it me?
                    if (workerUrl == ownUrl){

                        workerID = ownId;

                    } else {

                        foreach (KeyValuePair<int, string> workerPair in workersIDUrl) {

                            if (workerPair.Value == workerUrl) {

                                workerID = workerPair.Key;

                                break;
                            }

                        }
                    }

                    //Assign that job to this worker
                    freeWorkStatus.setWorkerId(workerID);

                    //System.Console.WriteLine("[DISTRIBUTE_WORK] Sent work to " + workerUrl);

                } catch (SocketException e) {

                    //System.Console.WriteLine("[ERROR_DISTRIBUTE_WORK1]  Cant send work to " + workerUrl);

                    availableWorkerToJobMutex.WaitOne();
                    availableWorkerToJob.Enqueue(workerUrl);
                    availableWorkerToJobMutex.ReleaseMutex();

                    queueWorkersSemaphore.Release(1);

                } finally {

                    jobsAssigmentMutex.ReleaseMutex();

                }
            }
        }
    }
}
