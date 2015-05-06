using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;

using PADIMapNoReduce;

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

        // This keeps the record of the jobs
        // already done and being done
        private JobDistribution jobDistribution;

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

            // Initialize the Job Distribution structure
            jobDistribution = new JobDistribution();

            System.Console.WriteLine("Created Client at: " + url);
        }

        public bool Submit(string entryUrl, string inputFilePath, string outputDirectoryPath, string className, string classImplementationPath, int numberOfSplits){

            this.entryUrl = entryUrl;
            this.inputFilePath = inputFilePath;
            this.outputDirectoryPath = outputDirectoryPath;
            this.className = className;
            this.classImplementationPath = classImplementationPath;
            this.numberOfSplits = numberOfSplits;

            jobDistribution.TotalJobs = numberOfSplits;
            jobDistribution.JobsDone = 0;

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
                
            } catch (SocketException e) {
                System.Console.WriteLine("[CLIENT_IMPLEMENTATION_ERROR1:SUBMIT] Could not request job.");
                System.Console.WriteLine(e.StackTrace);
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

            // Save the job distributed on our JobDistribution class
            System.Console.WriteLine("[GET_INPUT_SPLIT] Keeping track of the job...");
            jobDistribution.AddJob(workerId, inputBeginIndex, inputEndIndex);

            System.Console.WriteLine("[GET_INPUT_SPLIT] Finished Input Split");
            return new String(splitBuffer);
                        
        }

        public bool sendProcessedSplit(int workerId, IList<KeyValuePair<string, string>> result)
        {

            // Print the split received
            foreach (KeyValuePair<string, string> pair in result)
                Console.WriteLine("Received split for worker {0} | key: {1}; value: {2}", workerId, pair.Key, pair.Value);

            // Update the job result
            jobDistribution.UpdateJob(workerId, result);

            // Check if the whole job is done
            if (numberOfSplits == jobDistribution.JobsDone)
                finishJob();

            return true;
        }

        public bool finishJob() {

            // Retrieve all jobs from the Job Distribution

            // Export result to file
            
            // Inform the User Application of the final result if it exists

            Console.WriteLine("[FINISH_JOB] Finished job!");
            return true;
        }

    }
}
