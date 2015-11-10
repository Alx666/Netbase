using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Netbase.Shared
{
    public class SessionNonBlocking : ISession, IService, IDisposable
    {
        private Socket              m_hSocket;
        private static Interpreter  Interpreter;

        static SessionNonBlocking()
        {
            Interpreter = new Interpreter();
        }

        public SessionNonBlocking()
        {

        }

        public void Dispose()
        {
            
        }

        public void Connect(string sAddr, int iPort, Action<int> hAction)
        {
            m_hSocket           = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_hSocket.Blocking  = false;
            m_hSocket.BeginConnect(sAddr, iPort, OnEndConnect, m_hSocket);
        }


        public ushort Id
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IService Service
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Recycle(ISession hSession)
        {
            throw new NotImplementedException();
        }

        public void Send(Packet hPacket)
        {
            throw new NotImplementedException();
        }

        private void OnEndConnect(IAsyncResult hRes)
        {
            m_hSocket.EndConnect(hRes);

        }
    }
}
