using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PADIMapNoReduce;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;

namespace Client
{
    class ClientImplementation : MarshalByRefObject, IClient {

        private String url;

        public ClientImplementation(){

            // Get the IP's host
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    url = ip.ToString();
                }
            }

            // Prepend the protocol and append the port
            url = "tcp://" + url + ":" + NextFreeTcpPort() + "/C";

            // Console message
            Console.WriteLine("Created Client at " + url + ".");


        }

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

        public string getInputSplit(int workerId, long inputBeginIndex, long inputEndIndex)
        {
            return "";
        }

        public bool sendProcessedSplit(int workerID, IList<KeyValuePair<string, string>> result)
        {
            return false;
        }

        /* 
         * Utility functions
         */
        private int NextFreeTcpPort()
        {

            int portStartIndex = 10001;
            int portEndIndex = 19999;

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = portStartIndex; port <= portEndIndex; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    unusedPort = port;
                    break;
                }
            }

            return unusedPort;

        }
    }
}
