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

        private string entryUrl;
        private string inputFilePath;
        private string outputDirectoryPath;
        private string className;
        private string classImplementationPath;
        private int numberOfSplits;

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

            int tcpPort = int.Parse(System.Environment.GetEnvironmentVariable("ClientTcpPort", EnvironmentVariableTarget.Process));

            // Prepend the protocol and append the port
            url = "tcp://" + url + ":" + tcpPort + "/C";

            System.Console.WriteLine("Created Client at: " + url);
        }

        public bool Submit(string entryUrl, string inputFilePath, string outputDirectoryPath, string className, string classImplementationPath, int numberOfSplits){

            this.entryUrl = entryUrl;
            this.inputFilePath = inputFilePath;
            this.outputDirectoryPath = outputDirectoryPath;
            this.className = className;
            this.classImplementationPath = classImplementationPath;
            this.numberOfSplits = numberOfSplits;

            System.Console.WriteLine("[SUBMIT] Connecting to Job Tracker at " + entryUrl + ".");

            IWorker jobTrackerObj = (IWorker) Activator.GetObject(typeof(IWorker), entryUrl);

            Console.WriteLine("[SUBMIT] Connected!");

            // Send Class Implementation for workers
            // Get input file size
            long inputLength = new FileInfo(inputFilePath).Length;
            Console.WriteLine("[SUBMIT] Input length: " + inputLength + ".");


            // Get DLL bytecode
            byte[] dllCode = File.ReadAllBytes(classImplementationPath);
            Console.WriteLine("[SUBMIT] Read DLL file.");

            try {
                Console.WriteLine("[SUBMIT] Requesting job...");
                jobTrackerObj.RequestJob(url, inputLength, className, dllCode, numberOfSplits);        
                
            } catch (SocketException) {
                System.Console.WriteLine("[CLIENT_IMPLEMENTATION_ERR1] Could not request job.");
                return false;
            }

            return true;

        }

        public string getInputSplit(int workerId, long inputBeginIndex, long inputEndIndex)
        {
            System.Console.WriteLine("[GET_INPUT_SPLIT] Getting split for worker " + workerId + "[ " + inputBeginIndex + " , " + inputEndIndex + " ]..." );

            System.Console.WriteLine("[GET_INPUT_SPLIT] Openning File...");

            FileStream fs = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            BufferedStream bs = new BufferedStream(fs);

            // The StreamReader to read until the next "\n"
            // starting from the input's begin index
            StreamReader beginStreamReader = new StreamReader(bs);

            // Set the starting position of the StreamReader
            beginStreamReader.BaseStream.Position = inputBeginIndex;

            // Current character being analised
            char[] currentChar = new char[1];

            // If it's not the first block
            // i.e. the beggining of the file
            if (inputBeginIndex != 0) {

                // Read the next character from the StreamReader's buffer
                beginStreamReader.Read(currentChar, 0, 1);

                // Search for a newline while the block size is not reached
                while (currentChar[0] != '\n' && inputBeginIndex < inputEndIndex - 1) {
                    inputBeginIndex++;
                    beginStreamReader.Read(currentChar, 0, 1);
                }

            }

            // It is necessary to have a new StreamReader to read
            // from the input's end index until reaching a newline
            StreamReader endStreamReader = new StreamReader(bs);

            // Set the StreamReader's starting position
            endStreamReader.BaseStream.Position = inputEndIndex;

            // If the input's end index is not the end of the file
            if (inputEndIndex != fs.Length) {

                // Read the next character from the StreamReader's buffer
                endStreamReader.Read(currentChar, 0, 1);

                // Search for a newline while the end of the file is not reached
                while (currentChar[0] != '\n' && inputEndIndex <= fs.Length) {
                    inputEndIndex++;
                    endStreamReader.Read(currentChar, 0, 1);
                }

            }

            // Buffer to keep the whole split
            char[] splitBuffer = new char[inputEndIndex - inputBeginIndex];

            // Reset the beginStreamReader to the begging of the bock
            beginStreamReader.BaseStream.Position = inputBeginIndex;

            // Read the whole split from the StreamReader's buffer
            beginStreamReader.ReadBlock(splitBuffer, 0, splitBuffer.Length);

            System.Console.WriteLine("[GET_INPUT_SPLIT] Finnish Input Split");
            return new String(splitBuffer);
                        
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
