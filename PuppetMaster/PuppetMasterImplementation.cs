using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;

using PADIMapNoReduce;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;

namespace PuppetMaster
{
    class PuppetMasterImplementation : MarshalByRefObject, IPuppetMaster
    {

        private String url;

        public PuppetMasterImplementation() {

            // Get the IP's host
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    url = ip.ToString();
                }
            }

            // Prepend the protocol and append the port
            url = "tcp://" + url + ":" + NextFreeTcpPort() + "/PM" ;

            // Console message
            Console.WriteLine("Created PuppetMaster at " + url + ".");

        }

        public bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl)
        {

            // If the PuppetMaster is my own
            if (puppetMasterUrl.Equals(url))
            {
                // Start the worker process
                string workerExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Worker\\bin\\Debug\\Worker.exe");

                Process.Start(workerExecutablePath, id + " " + serviceUrl);

                // Console message
                Console.WriteLine("Created worker " + id + " at " + serviceUrl + ".");

                return true;

            } else {

            }

            return false;

        }

        public bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl, string entryUrl)
        {
            // If the PuppetMaster is my own
            if (puppetMasterUrl.Equals(url))
            {
                // Start the worker process
                string workerExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Worker\\bin\\Debug\\Worker.exe");

                // Console message
                Console.WriteLine("Created worker " + id + " at " + serviceUrl + ". Entry Level:" + entryUrl + ".");

                Process.Start(workerExecutablePath, id + " " + serviceUrl + " " + entryUrl);

                

                return true;

            } else {



            }

            return false;
        }

        public bool SubmitJob(string entryUrl, string file, string output, string s, string map) {
            /*
             // Start the client application
                string clientExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Client\\bin\\Debug\\Client.vshost.exe");

                // Console message
                Console.WriteLine("Created client application to submit job request " + file + "at worker node in url " +  entryUrl + " with " + s + " splits and " + map + " dll.");

                Process.Start(clientExecutablePath, file + " " + entryUrl + " " + s + " " + map);
             
             
             */

            return true; }

        public bool Wait(int seconds) { return true; }

        public bool Status() { 
            /*
             this.workerList.first().PrintStatus();
             */
            return true; }

        public bool SlowW(string id, int delayInSeconds) {
            /*
            this.workerList.first().SlowWorker(id, delayInSeconds);
            */
            return true;
        }

        public bool FrezeeW(string id) {
            /*
            this.workerList.first().FreezeWorker(id);
            */
            return true; }

        public bool UnfreezeW(string id) {
            /*
            this.workerList.first().UnFreezeWorker(id);
            */
            return true;
        }

        public bool FreezeC(string id){
            /*
            this.workerList.first().FreezeWorkerComunication(id);
            */
            return true;
        }

        public bool UnfreezeC(string id) {
            /*
            this.workerList.first().UnFreezeWorkerComunication(id);
            */
            return true;
        }

        /* 
         * Utility functions
         */
        private int NextFreeTcpPort() {

            int portStartIndex = 20001;
            int portEndIndex = 29999;

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = portStartIndex; port <= portEndIndex; port++) {
                if (!usedPorts.Contains(port)) {
                    unusedPort = port;
                    break;
                }
            }

            return unusedPort;
        
        }

    }
}
