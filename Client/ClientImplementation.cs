using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PADIMapNoReduce;

namespace Client
{
    class ClientImplementation : MarshalByRefObject, IClient{


        bool Submit(string inputFilePath, string outputDirectoryPath, string classImplementationPath, int numberOfSplits){
            

        }

        string getInputSplit(int workerId, int inputBeginIndex, int inputEndIndex)
        {
            
        }

        bool sendProcessedSplit(int workerID, IList<KeyValuePair<string, string>> result)
        {
            
        }
    }
}
