using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;

using PADIMapNoReduce;
using System.Diagnostics;


namespace Client {

    class Client {

        static void Main(string[] args) {

            Uri clientUri = new Uri(args[0]);

            int tcpPort = clientUri.Port;

            String[] segments = args[0].Split('/');
            String remoteObjectName = segments[segments.Length - 1];

            TcpChannel channel = new TcpChannel(tcpPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientImplementation), remoteObjectName, WellKnownObjectMode.Singleton);

            System.Environment.SetEnvironmentVariable("ClientTcpPort", tcpPort.ToString(), EnvironmentVariableTarget.Process);

            System.Console.WriteLine("Created Client on " + clientUri.ToString() );
            System.Console.WriteLine("Press <enter> to terminate client...");
            System.Console.ReadLine();
        }

    }
}
