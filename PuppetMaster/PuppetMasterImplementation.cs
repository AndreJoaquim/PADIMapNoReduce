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
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace PuppetMaster
{
    class PuppetMasterImplementation : MarshalByRefObject, IPuppetMaster
    {

        private String mIp;
        private String url;
        private int tcpPort;

        public PuppetMasterImplementation() {

            // Get the IP's host
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    mIp = ip.ToString();
                }
            }

            String remoteObjectName = "PM";

            // Prepend the protocol and append the port
            tcpPort = int.Parse(System.Environment.GetEnvironmentVariable("PM_Port", EnvironmentVariableTarget.Process));
            url = "tcp://" + mIp + ":" + tcpPort + "/" + remoteObjectName;

        }

        public bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl)
        {

            // Convert the localhost's to local IP address
            Uri puppetMasterUri = ConvertLocalhostInUri(puppetMasterUrl);
            Uri serviceUri = ConvertLocalhostInUri(serviceUrl);

            // If the PuppetMaster is my own
            if (puppetMasterUri.Equals(url))
            {
                // Start the worker process
                string workerExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Worker\\bin\\Debug\\Worker.exe");

                Process.Start(workerExecutablePath, id + " " + serviceUri.ToString());

                // Console message
                Console.WriteLine("Created worker " + id + " at " + serviceUri.ToString() + ".");

                return true;

            } else {

                // Retrieve the remote Puppet Master object
                IPuppetMaster puppetMasterObj = (IPuppetMaster) Activator.GetObject(typeof(IPuppetMaster), puppetMasterUri.ToString());

                // Create the worker in the remote Puppet Master
                puppetMasterObj.CreateWorker(id, puppetMasterUri.ToString(), serviceUri.ToString());

            }

            return false;

        }

        public bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl, string entryUrl)
        {

            // Convert the localhost's to local IP address
            Uri puppetMasterUri = ConvertLocalhostInUri(puppetMasterUrl);
            Uri serviceUri = ConvertLocalhostInUri(serviceUrl);
            Uri entryUri = ConvertLocalhostInUri(entryUrl);

            // If the PuppetMaster is my own
            if (puppetMasterUri.Equals(url))
            {
                // Start the worker process
                string workerExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Worker\\bin\\Debug\\Worker.exe");

                // Console message
                Console.WriteLine("Created worker " + id + " at " + serviceUri.ToString() + ". Entry Level:" + entryUri.ToString() + ".");
                Process.Start(workerExecutablePath, id + " " + serviceUri.ToString() + " " + entryUri.ToString());

                return true;

            } else {

                // Retrieve the remote Puppet Master object
                IPuppetMaster puppetMasterObj = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), puppetMasterUri.ToString());

                // Create the worker in the remote Puppet Master
                puppetMasterObj.CreateWorker(id, puppetMasterUri.ToString(), serviceUri.ToString(), entryUri.ToString());

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

        private Uri ConvertLocalhostInUri(String url) {

            Uri uri = new Uri(url);

            // Check if the host is the local machine i.e. localhost
            if (uri.Host.Equals("localhost"))
            {
                UriBuilder builder = new UriBuilder(uri);
                builder.Host = mIp;
                uri = builder.Uri;
            }

            return uri;

        }

    }
}
