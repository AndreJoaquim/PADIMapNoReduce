using System;
using System.Collections.Generic;


namespace PADIMapNoReduce {
    public interface IMapper {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

    public interface IMapperTransfer {
        bool SendMapper(byte[] code, string className);
    }

    public interface IWorker : IMapperTransfer {

        bool RegisterOwnWorker(string url);

        bool BroadcastNewWorker(string url);

        bool RegisterNewWorkers(List<string> workerServiceUrl);

        bool PrintStatus();
    }


    public interface IClient {

        bool Submit(String inputFilePath, String outputDirectoryPath, String classImplementationPath, int numberOfSplits);

        String getInputSplit(int workerId, int inputBeginIndex, int inputEndIndex);

        bool sendProcessedSplit(int workerID, IList<KeyValuePair<string, string>> result);
        
 
    }

    public interface IPuppetMaster {

        bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl);

        bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl, string entryUrl);

        bool SubmitJob(string entryUrl, string file, string output, string s, string map);

        bool Wait(int seconds);

        bool Status();

        bool SlowW(string id, int delayInSeconds);

        bool FrezeeW(string id);

        bool UnfreezeW(string id);

        bool FreezeC(string id);

        bool UnfreezeC(string id);
    
    }
}
