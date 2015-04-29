using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;

using PADIMapNoReduce;


namespace Client {
    class Client {

        static void Main(string[] args) {

            IClient clientImpl = new ClientImplementation();

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

        }

    }
}
