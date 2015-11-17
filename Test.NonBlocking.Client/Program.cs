using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Test.NonBlocking.Protocol;

namespace Test.NonBlocking.Client
{
    internal class Client : NonBlockingTestCallback
    {
        private Thread m_hThread;

        public Client()
        {
            m_hThread = new Thread(ThreadRoutine);
            m_hThread.Start();
        }


        public override string OnForwardTestMessage(string sMessage)
        {
            return null;
        }

        private void ThreadRoutine()
        {
            while (true)
            {
                this.Update();
            }
        }
    }

    class Program
    {
        private static int Connections = 500;
        private static Client m_hClient;
        private static AutoResetEvent m_hEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {

            m_hClient = new Client();
            m_hClient.Connected     += OnConnect;
            m_hClient.Disconnected  += OnDisconnect;

            Console.WriteLine("Testing Connect/Disconnect");
            OnDisconnect();

            m_hEvent.WaitOne();

        }

        private static void OnDisconnect()
        {
            if(Connections > 0)
                m_hClient.Connect("127.0.0.1", 28000);
        }

        private static void OnConnect()
        {
            Connections--;
            Console.CursorLeft = 0;
            Console.Write(Connections);

            if (Connections > 0)
                m_hClient.Dispose();
            else
                m_hEvent.Set();
        }
    }
}
