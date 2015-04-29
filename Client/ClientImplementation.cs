using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PADIMapNoReduce;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.Net.Sockets;

namespace Client
{
    class ClientImplementation : MarshalByRefObject, IClient{

        public bool Submit(string entryUrl, string inputFilePath, string outputDirectoryPath, string className, string classImplementationPath, int numberOfSplits){
            
            //Get jobtracker
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            IWorker jobTrackerObj = (IWorker)Activator.GetObject(typeof(IWorker), entryUrl);

            //Send Class Implementation for workers
            //Get input file size
            long inputLength = new FileInfo(inputFilePath).Length;

            //Get DLL bytecode
            byte[] dllCode = File.ReadAllBytes(classImplementationPath);

            try {
            
                jobTrackerObj.RequestJob(inputLength,className, dllCode, numberOfSplits);        
                
            } catch (SocketException) {
                System.Console.WriteLine("[CLIENT_IMPLEMENTATION1] Could not request job");
            }

            return true;

        }

        public string getInputSplit(int workerId, int inputBeginIndex, int inputEndIndex)
        {
            return "";
        }

        public bool sendProcessedSplit(int workerID, IList<KeyValuePair<string, string>> result)
        {
            return false;
        }
    }
}
