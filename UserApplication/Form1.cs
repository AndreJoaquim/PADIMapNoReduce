using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

using PADIMapNoReduce;

namespace UserApplication
{
    public partial class UserApplication : Form
    {

        private String inputFilePath;
        private String outputDirectoryPath;
        private String classImplementationPath;

        private String clientUrl;

        public UserApplication()
        {
            InitializeComponent();

            // Get the IP's host
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    clientUrl = ip.ToString();
                }
            }

            // Prepend the protocol and append the port
            clientUrl = "tcp://" + clientUrl + ":" + NextFreeTcpPort() + "/C";

            // Start the worker process
            string clientExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Client\\bin\\Debug\\Client.exe");
            Process.Start(clientExecutablePath, clientUrl);

        }

        public void JobConcluded() {

            labelProgress.ForeColor = Color.Green;
            labelProgress.Text = "Job concluded!";

        }

        private void btInputFile_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = Application.StartupPath;

            if (dialog.ShowDialog() == DialogResult.OK){

                inputFilePath = dialog.FileName;
                lbInputFilePath.Text = inputFilePath;
            }

        }

        private void btOutputDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            
            dialog.SelectedPath = Application.StartupPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {

                outputDirectoryPath = dialog.SelectedPath;
                lbOutputDirectory.Text = outputDirectoryPath;
            }
        }

        private void btClass_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = Application.StartupPath;
            dialog.Filter = "DLL Files (*.dll)|*.dll|All files (*.*)|*.*";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                classImplementationPath = dialog.FileName;
                lbClass.Text = classImplementationPath;
            }
        }

        private void btSubmit_Click(object sender, EventArgs e)
        {

            // Retrieve class name
            String className = tbClassName.Text.ToString();

            // Retrieve number of splits
            int numberOfSplits = Int32.Parse(tbNrSplits.Text.ToString());

            // Retrieve entry Url
            String entryUrl = tbEntryUrl.Text.ToString();

            // Call the client remote object
            IClient clientObj = (IClient) Activator.GetObject(typeof(IClient), clientUrl);

            // Verify number of splits >= 1
 
            // Call the client with the submit call
            bool value = clientObj.Submit(entryUrl, inputFilePath, outputDirectoryPath, className, classImplementationPath, numberOfSplits);
            
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
