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

        public bool Submit(string inputFilePath, string outputDirectoryPath, string classImplementationPath, int numberOfSplits){
            
            //Send Class Implementation for workers
            string classMapperName = Path.GetFileNameWithoutExtension(classImplementationPath);
            
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            IWorker mt = (IWorker)Activator.GetObject(typeof(IWorker), "tcp://localhost:10000/W");
            try {
                byte[] code = File.ReadAllBytes(classImplementationPath);
                mt.SendMapper(code, classMapperName);
            } catch (SocketException) {
                System.Console.WriteLine("Could not locate server");
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
