using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Netbase.Shared
{
      
    public class SessionNonBlocking : ISession, IService, IDisposable
    {        
        public event Action Connected;
        public event Action Disconnected;

        public IService             Service { get { return this; } }

        private Socket              m_hSocket;
        private MemoryStream        m_hMs;
        private BinaryReader        m_hReader;
        private byte[]              m_hRecvBuffer;
        private Queue<Packet>       m_hToSend;
        protected static Interpreter  Interpreter;

        private Packet m_hCurrentPacket;
        private int m_iSendOffset;

        private ISessionState                   m_hCurrentState;
        private SessionStateDisconnected        m_hDisconnected;
        private SessionStateConnecting          m_hConnecting;
        private SessionStateJustConnected       m_hJustConnected;
        private SessionStateActive              m_hActive;
        private SessionStateJustDisconnected    m_hJustDisconnected;



        static SessionNonBlocking()
        {
            Interpreter = new Interpreter();
        }

        public SessionNonBlocking()
        {
            m_hRecvBuffer       = new byte[1024];
            m_hMs               = new MemoryStream(m_hRecvBuffer);
            m_hReader           = new BinaryReader(m_hMs);
            m_hToSend           = new Queue<Packet>();
            m_hDisconnected     = new SessionStateDisconnected(this);
            m_hConnecting       = new SessionStateConnecting(this);
            m_hJustConnected    = new SessionStateJustConnected(this);
            m_hActive           = new SessionStateActive(this);
            m_hJustDisconnected = new SessionStateJustDisconnected(this);

            m_hDisconnected.Next            = m_hConnecting;
            m_hConnecting.Success           = m_hJustConnected;
            m_hConnecting.Fail              = m_hJustDisconnected;
            m_hJustConnected.Next           = m_hActive;
            m_hJustDisconnected.Next        = m_hDisconnected;
            m_hActive.Fail                  = m_hJustDisconnected;
            m_hCurrentState                 = m_hDisconnected;
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            if (m_hSocket != null)
            {
                m_hSocket.Shutdown(SocketShutdown.Both);
                m_hSocket.Close();
                m_hSocket = null;
            }
        }

        public void Update()
        {
            Interlocked.Exchange(ref m_hCurrentState, m_hCurrentState.Update());
            Thread.Sleep(10);
            //m_hCurrentState = m_hCurrentState.Update();
        }

        public void Connect(string sAddr, int iPort)
        {
            if (m_hSocket == null)
            {
                m_hDisconnected.Connect(sAddr, iPort);
                Interlocked.Exchange(ref m_hCurrentState, m_hDisconnected);
            }
            else
            {
                throw new SocketException();
            }         
        }
        
        public void Send(Packet hPacket)
        {
            m_hToSend.Enqueue(hPacket);
        }

        private static SocketError ReceiveAll(Socket hSocket, byte[] hDataBuffer, out byte bPacketId)
        {
            SocketError eError;
            int iReceived;
            bPacketId = 0;

            //Get Header
            iReceived = hSocket.Receive(hDataBuffer, 0, Packet.HeaderSize, SocketFlags.None, out eError);
            if (eError == SocketError.WouldBlock)
                return eError;
            else if (iReceived == 0 || eError != SocketError.Success)
                throw new SocketException();

            ushort uSize = BitConverter.ToUInt16(hDataBuffer, 1);
            int iOffet = Packet.HeaderSize;
            int iTotalRecv = 0;

            while (iTotalRecv < uSize)
            {
                iReceived = hSocket.Receive(hDataBuffer, iOffet, uSize - iTotalRecv, SocketFlags.None, out eError);
                if (eError == SocketError.WouldBlock)
                    return eError;
                else if (iReceived == 0 || eError != SocketError.Success)
                    throw new SocketException();

                iTotalRecv += iReceived;
                iOffet += iReceived;
            }

            bPacketId = hDataBuffer[0];
            return eError;
        }

        #region Nested Types

        private interface ISessionState
        {
            ISessionState Update();
        }

        private class SessionStateDisconnected : ISessionState
        {
            private SessionNonBlocking m_hOwner;
            private bool m_bStartConnect;

            public SessionStateConnecting Next { get; set; }

            public SessionStateDisconnected(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public void Connect(string sAddr, int iPort)
            {
                Next.Init(sAddr, iPort);
                m_bStartConnect = true;
            }

            public ISessionState Update()
            {
                if (m_bStartConnect)
                {
                    m_bStartConnect = false;
                    return Next;
                }
                else
                {
                    return this;
                }                
            }
        }

        private class SessionStateConnecting : ISessionState
        {
            private SessionNonBlocking m_hOwner;
            private bool m_bConnected;
            private bool m_bFail;

            public SessionStateJustConnected Success { get; set; }
            public SessionStateJustDisconnected Fail { get; set; }

            public SessionStateConnecting(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public void Init(string sAddr, int iPort)
            {
                m_bConnected    = false;
                m_bFail         = false;
                m_hOwner.m_hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_hOwner.m_hSocket.BeginConnect(sAddr, iPort, OnEndConnect, m_hOwner);
            }

            private void OnEndConnect(IAsyncResult hRes)
            {
                try
                {
                    m_hOwner.m_hSocket.EndConnect(hRes);
                    m_hOwner.m_hSocket.Blocking = false;
                    m_bConnected = true;
                }
                catch (Exception)
                {
                    m_bFail = true;
                }
            }

            public ISessionState Update()
            {
                if (m_bConnected)
                    return Success;
                else if (m_bFail)
                    return Fail;
                else
                    return this;
            }
        }

        private class SessionStateJustConnected : ISessionState
        {
            public SessionStateActive Next { get; set; }

            private SessionNonBlocking m_hOwner;

            public SessionStateJustConnected(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public ISessionState Update()
            {
                if (m_hOwner.Connected != null)
                    m_hOwner.Connected();

                return Next;
            }
        }

        private class SessionStateJustDisconnected : ISessionState
        {
            public ISessionState Next { get; set; }

            private SessionNonBlocking m_hOwner;

            public SessionStateJustDisconnected(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public ISessionState Update()
            {
                if (m_hOwner.m_hSocket != null)
                {
                    m_hOwner.m_hSocket.Shutdown(SocketShutdown.Both);
                    m_hOwner.m_hSocket.Close();
                    m_hOwner.m_hSocket = null;
                }

                if (m_hOwner.Disconnected != null)
                    m_hOwner.Disconnected();

                return Next;
            }
        }

        private class SessionStateActive : ISessionState
        {
            private SessionNonBlocking m_hOwner;
            public SessionStateJustDisconnected Fail { get; set; }


            public SessionStateActive(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public ISessionState Update()
            {
                try
                {
                    SocketError eError = SocketError.Success;

                    while (m_hOwner.m_hToSend.Count > 0 && eError != SocketError.WouldBlock)
                    {
                        if (m_hOwner.m_hCurrentPacket == null)
                        {
                            m_hOwner.m_hCurrentPacket = m_hOwner.m_hToSend.Dequeue();
                            m_hOwner.m_iSendOffset = 0;
                        }

                        int iTotal = m_hOwner.m_hCurrentPacket.DataSize + Packet.HeaderSize;
                        m_hOwner.m_iSendOffset += m_hOwner.m_hSocket.Send(m_hOwner.m_hCurrentPacket.Buffer, m_hOwner.m_iSendOffset, iTotal - m_hOwner.m_iSendOffset, SocketFlags.None, out eError);

                        if (m_hOwner.m_iSendOffset == iTotal)
                        {
                            if(m_hOwner.m_hCurrentPacket.Pool != null)
                                m_hOwner.m_hCurrentPacket.Recycle();

                            m_hOwner.m_hCurrentPacket = null;
                        }
                    }

                    byte bPacketId;
                    while (ReceiveAll(m_hOwner.m_hSocket, m_hOwner.m_hRecvBuffer, out bPacketId) != SocketError.WouldBlock)
                    {
                        //Portiamo lo stream in posizione per leggere i dati
                        m_hOwner.m_hMs.Seek(Packet.DataSizeIndex, SeekOrigin.Begin);

                        //Dobbiamo implementare il meccanismo per prendere il pacchetto giusto in base all'id ricevuto
                        IAction hAction = Interpreter.Get(bPacketId);

                        //Carichiamo la action
                        hAction.LoadData(m_hOwner.m_hReader);

                        //Eseguiamo l'action                    
                        hAction.Execute(m_hOwner, m_hOwner);
                    }

                    return this;
                }
                catch (Exception hEx)
                {
                    Console.WriteLine(hEx);
                    return Fail;
                }                
            }
        }

        #endregion

    }
}
