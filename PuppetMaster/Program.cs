using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace PuppetMaster
{
    class Program
    {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.Run(new ScriptLoader());
        }
    }

}
