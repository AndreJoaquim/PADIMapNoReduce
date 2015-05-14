using System;
using System.Collections.Generic;


namespace PADIMapNoReduce {
    public interface IMapper {

        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

    public interface IWorker {

        // Job Tracker Methods
        bool RequestJob(string clientUrl, long inputSize, string className, byte[] dllCode, int NumberOfSplits);

        bool FinishProcessing(string workerUrl, WorkStatus workerStatus);

        bool isSplitValid(int workerId, long beginSplit);

        bool IsJobTrackerAlive(int workerId, string workerUrl, bool working);

        //Worker Methods
        bool RunJob(string className, byte[] dllCode, long beginIndex, long endIndex, string clientUrl, string jobTackerUrl);

        bool RegisterOwnWorker(int id, string url);

        bool BroadcastNewWorker(int id, string url);

        bool RegisterNewWorkers(HashSet<KeyValuePair<int,string>> workerServiceUrl);

        bool IsNodeInNetwork(KeyValuePair<int,string> node);

        bool PrintStatus();

        bool SlowWorker(int delay);

        bool FreezeW();

        bool UnfreezeW();
        
        bool FreezeC();
        
        bool UnfreezeC();

        WorkStatus IsAlive(HashSet<KeyValuePair<int, string>> currentViewWorkers, HashSet<WorkStatus> currentJobAssigment);

    }

    public interface IClient {

        bool Submit(String entryUrl, String inputFilePath, String outputDirectoryPath, String className, String classImplementationPath, int numberOfSplits);

        String getInputSplit(int workerId, long inputBeginIndex, long inputEndIndex);

        bool sendProcessedSplit(int workerId, String result);

        bool finishJob();
        
    }

    public interface IPuppetMaster {

        bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl);

        bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl, string entryUrl);

        bool SubmitJob(String entryUrl, String inputFilePath, String outputDirectoryPath, String className, String classImplementationPath, int numberOfSplits);

        bool Wait(int seconds);

        bool Status();

        bool SlowW(string id, int delayInSeconds);

        bool FrezeeW(string id);

        bool UnfreezeW(string id);

        bool FreezeC(string id);

        bool UnfreezeC(string id);
    
    }
}
