using System;
using System.Net;
using System.Threading;

namespace Test.LongCalls.Server
{
    internal class LongCallsContex : Test.LongCalls.Service.LongCallsServiceContext
    {

    }

    internal class LongCallsServer : Test.LongCalls.Service.LongCallsService<LongCallsContex>
    {
        public override int TimedCall(LongCallsContex hContex, int iSeconds)
        {
            //Console.WriteLine("Starting " + iSeconds + " seconds work");
            Thread.Sleep(iSeconds * 100);
            //Console.WriteLine(iSeconds + " work complete");
            return iSeconds;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            using (LongCallsServer hServer = new LongCallsServer())
            {
                hServer.ClientConnected += OnClientConnected;
                hServer.ClientDisconnected += OnClientDisconnected;
                hServer.Start(IPAddress.Any, 28000);
                hServer.WaitForExit();
            }
        }

        private static void OnClientDisconnected(LongCallsContex hContext, System.Exception hEx, System.Net.Sockets.SocketError eErr)
        {
            Console.WriteLine("Client Disconnected");
        }

        private static void OnClientConnected(LongCallsContex hContext)
        {
            Console.WriteLine("Client Connected");
        }
    }
}
