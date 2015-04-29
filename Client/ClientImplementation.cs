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

        private static FileStream fs;

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
            
                jobTrackerObj.RequestJob(url,inputLength,className, dllCode, numberOfSplits);        
                
            } catch (SocketException) {
                System.Console.WriteLine("[CLIENT_IMPLEMENTATION1] Could not request job");
            }

            fs = new FileStream(inputFilePath, FileMode.Open);

            return true;

        }

        public string getInputSplit(int workerId, long inputBeginIndex, long inputEndIndex)
        {



            byte[] bytes = new byte[inputEndIndex - inputBeginIndex];



            fs.Read(bytes, (int)inputBeginIndex, bytes.Length);


            string singlechar;

            if (inputBeginIndex != 0)
            {
                int i = 0;
                singlechar = Encoding.UTF8.GetString(bytes, i, 1);
                while (!singlechar.Equals('\n'))
                {
                    inputBeginIndex++;
                    i++;
                    singlechar = Encoding.UTF8.GetString(bytes, i, 1);
                }
                inputBeginIndex++;

            }

            singlechar = Encoding.UTF8.GetString(bytes, bytes.Length-1, 1);

            byte[] temp = new byte[1];

            byte[] temp2 = bytes;

            byte[] temp3;


            
            while (!singlechar.Equals('\n'))
            {
                fs.Read(temp, (int)inputEndIndex, 1);
                singlechar = Encoding.UTF8.GetString(temp, temp.Length - 1, 1);


                temp3 = new byte[temp2.Length + temp.Length];
                temp2.CopyTo(temp3, 0);
                temp.CopyTo(temp3, temp2.Length);

                temp2 = temp3;

            }

            temp3 = new byte[temp2.Length + temp.Length];
            temp2.CopyTo(temp3, 0);
            temp.CopyTo(temp3, temp2.Length);

            temp2 = temp3;

            byte[] sub = SubArray(temp2, (int)inputBeginIndex);


            string s = Encoding.UTF8.GetString(sub, 0, sub.Length);

            return s;
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

        public byte[] SubArray(byte[] data, int index)
        {
            byte[] result = new byte[data.Length-index];
            data.CopyTo(result, index);
            return result;
        }
    }
}
