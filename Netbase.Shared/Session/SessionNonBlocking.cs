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

        private Packet m_hCurrent;
        private int m_iSendOffset;

        private ISessionState m_hCurrentState;
        private ISessionState m_hDisconnected;
        private ISessionState m_hConnecting;
        private ISessionState m_hActive;



        static SessionNonBlocking()
        {
            Interpreter = new Interpreter();
        }

        public SessionNonBlocking()
        {
            m_hRecvBuffer   = new byte[1024];
            m_hMs           = new MemoryStream(m_hRecvBuffer);
            m_hReader       = new BinaryReader(m_hMs);
            m_hToSend       = new Queue<Packet>();

            m_hDisconnected = new SessionStateDisconnected(this);
            m_hConnecting   = new SessionStateConnecting(this);
            m_hActive       = new SessionStateActive(this);

            m_hDisconnected.Next                          = m_hConnecting;
            m_hConnecting.Next                            = m_hActive;
            m_hActive.Next                                = m_hDisconnected;

            m_hCurrentState                             = m_hDisconnected;
        }


        public void Update()
        {
            m_hCurrentState = m_hCurrentState.Update();

            if (m_hSocket.Connected)
            {
                try
                {
                    SocketError eError = SocketError.Success;

                    while (m_hToSend.Count > 0 && eError != SocketError.WouldBlock)
                    {
                        if(m_hCurrent == null)
                        {
                            m_hCurrent      = m_hToSend.Dequeue();
                            m_iSendOffset   = 0;
                        }

                        int iTotal = m_hCurrent.DataSize + Packet.HeaderSize;
                        m_iSendOffset += m_hSocket.Send(m_hCurrent.Buffer, m_iSendOffset, iTotal - m_iSendOffset, SocketFlags.None);

                        if (m_iSendOffset == iTotal)
                        {
                            m_hCurrent = null;
                            m_hCurrent.Recycle();
                        }
                    }

                    byte bPacketId;
                    while(ReceiveAll(this.m_hSocket, m_hRecvBuffer, out bPacketId) != SocketError.WouldBlock)
                    {
                        //Portiamo lo stream in posizione per leggere i dati
                        m_hMs.Seek(Packet.DataSizeIndex, SeekOrigin.Begin);

                        //Dobbiamo implementare il meccanismo per prendere il pacchetto giusto in base all'id ricevuto
                        IAction hAction = Interpreter.Get(bPacketId);

                        //Carichiamo la action
                        hAction.LoadData(m_hReader);

                        //Eseguiamo l'action                    
                        hAction.Execute(this, this);
                    }
              
                }
                catch (Exception)
                {
                    if (Disconnected != null)
                        Disconnected();
                }
            }     
        }

        public void Dispose()
        {
            m_hSocket.Shutdown(SocketShutdown.Both);
            m_hSocket.Close();
        }


        public void Connect(string sAddr, int iPort)
        {
            m_hSocket           = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_hCurrentState     = m_hConnecting;
            m_hSocket.BeginConnect(sAddr, iPort, OnEndConnect, m_hSocket);            
        }


        private void OnEndConnect(IAsyncResult hRes)
        {
            try
            {
                m_hSocket.EndConnect(hRes);
                m_hSocket.Blocking  = false;
                m_hCurrentState     = m_hActive;
            }
            catch (Exception)
            {
            }
            
        }

        //TODO: in modalità non di blocco, il metodo Send può risultare completato anche se invia una quantità di byte inferiore al numero di byte presente nel buffer.L'applicazione deve tenere traccia del numero di byte e ritentare l'operazione fino all'invio dei byte nel buffer
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
            ISessionState Next { get; set; }
        }

        private class SessionStateDisconnected : ISessionState
        {
            private SessionNonBlocking m_hOwner;
            public ISessionState Next { get; set; }

            public SessionStateDisconnected(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public ISessionState Update()
            {
                return this;
            }
        }

        private class SessionStateConnecting : ISessionState
        {
            public ISessionState Next { get; set; }
            private SessionNonBlocking m_hOwner;


            public SessionStateConnecting(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public ISessionState Update()
            {
                return this;
            }
        }

        private class SessionStateActive : ISessionState
        {
            public ISessionState Next { get; set; }
            private SessionNonBlocking m_hOwner;
        
            public SessionStateActive(SessionNonBlocking hOwner)
            {
                m_hOwner = hOwner;
            }

            public ISessionState Update()
            {
                return this;
            }
        }

        #endregion

    }
}
