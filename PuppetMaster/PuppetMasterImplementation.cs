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
                Process.Start("Worker/bin/Worker.exe", id + " " + puppetMasterUrl + " " + serviceUrl);

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
                Process.Start("Worker/bin/Worker.exe", id + " " + puppetMasterUrl + " " + serviceUrl + " " + entryUrl);
                return true;

            } else {



            }

            return false;
        }

        public bool SubmitJob(string entryUrl, string file, string output, string s, string map) { return true; }

        public bool Wait(int seconds) { return true; }

        public bool Status() { return true; }

        public bool SlowW(string id, int delayInSeconds) { return true; }

        public bool FrezeeW(string id) { return true; }

        public bool UnfreezeW(string id) { return true; }

        public bool FreezeC(string id) { return true; }

        public bool UnfreezeC(string id) { return true; }

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
