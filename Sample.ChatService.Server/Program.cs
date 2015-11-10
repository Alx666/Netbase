using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Sample.ChatService.Service;

namespace Sample.ChatService.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MyChatServer hServer = new MyChatServer())
            {
                hServer.ClientConnected     += OnClientConnected;
                hServer.ClientDisconnected  += OnClientDisconnected;
                hServer.Start(IPAddress.Parse("0.0.0.0"), 6666);
                hServer.WaitForExit();
            }                        
        }

        private static void OnClientConnected(MySessionHandler hContext)
        {
            Console.WriteLine("Client Connected");
        }

        private static void OnClientDisconnected(MySessionHandler hContext, Exception hEx, SocketError eErr)
        {
            Console.WriteLine("Client Disconnected" + "\nReason: " + hEx);
        }
    }
}
