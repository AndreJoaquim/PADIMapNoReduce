using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UserApplication
{
    public partial class UserApplication : Form
    {

        private String inputFilePath;
        private String outputDirectoryPath;
        private String ClassImplementationPath;


        public UserApplication()
        {
            InitializeComponent();

            // Start the worker process
            string clientExecutablePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Client\\bin\\Debug\\Client.exe");
            Process.Start(clientExecutablePath);

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
            dialog.Filter = "C Sharp Class Files (*.cs)|*.cs|All files (*.*)|*.*";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                ClassImplementationPath = dialog.FileName;
                lbClass.Text = ClassImplementationPath;
            }
        }

        private void btSubmit_Click(object sender, EventArgs e)
        {

            // Retrieve class name
            String className = tbClassName.Text.ToString();

            // Retrieve number of splits
            String numberOfSplits = tbNrSplits.Text.ToString();

            // Retrieve entry Url
            String entryUrl = tbEntryUrl.Text.ToString();

            // Retrieve the client remote object


            // Call the client with the submit call

        }

    }
}
