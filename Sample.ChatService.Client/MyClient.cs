using System;
using Sample.ChatService.Protocol;
using System.Threading;

namespace Sample.ChatService.Client
{
    internal class MyClient : ConcurrentChatSession
    {
        public override void OnForwardMessage(string sSender, string sMessage)
        {
            Console.WriteLine(sSender + ": " + sMessage);
        }
    }

    internal class MyNonBlockingClient : NonBlockingChatSession
    {
        private Thread m_hUpdateThread;

        public MyNonBlockingClient()
        {
            m_hUpdateThread = new Thread(ThreadRoutine);
            m_hUpdateThread.Start();
        }

        public override void OnForwardMessage(string sSender, string sMessage)
        {
            Console.WriteLine(sSender + ": " + sMessage);
        }

        private void ThreadRoutine()
        {
            while (true)
            {
                this.Update();
                Thread.Sleep(100);
            }
        }
    }
}
