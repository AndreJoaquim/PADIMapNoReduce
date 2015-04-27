using System;
using System.Collections.Generic;


namespace PADIMapNoReduce {
    public interface IMapper {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

    public interface IMapperTransfer {
        bool SendMapper(byte[] code, string className);
    }

    public interface IWorker : IMapperTransfer { }


    public interface IClient {

        bool Submit(String inputFilePath, String outputDirectoryPath, String classImplementationPath, int numberOfSplits);

        String getInputSplit(int workerId, int inputBeginIndex, int inputEndIndex);

        bool sendProcessedSplit(int workerID, IList<KeyValuePair<string, string>> result);
        
 
    }

    public interface IPuppetMaster {

        bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl);

        bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl, string entryUrl);
    
    }
}
