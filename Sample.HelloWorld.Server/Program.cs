using System;
using System.Net;

namespace Sample.HelloWorld.Server
{
    internal class HelloWorldContex : Sample.HelloWorld.Service.HelloWorldServiceContext
    {
    }

    internal class HelloWorldServer : HelloWorld.Service.HelloWorldService<HelloWorldContex>
    {
        public override string GetMessageFromServer(HelloWorldContex hContex)
        {
            Console.WriteLine("Client Requested Hello World");
            return "Hello World";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (HelloWorldServer hServer = new HelloWorldServer())
            {
                hServer.Start(IPAddress.Any, 28000);
                hServer.ClientDisconnected += ClientDisconnected;
                hServer.WaitForExit();
            }
        }

        private static void ClientDisconnected(HelloWorldContex hContext, Exception hEx, System.Net.Sockets.SocketError eErr)
        {
            Console.WriteLine("Client Disconnected");
        }
    }       
}
