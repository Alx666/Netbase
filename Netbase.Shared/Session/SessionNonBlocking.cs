﻿using System;
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
        private ISessionEvent       m_hEvent;
        protected static Interpreter  Interpreter;
        

        static SessionNonBlocking()
        {
            Interpreter = new Interpreter();
        }

        public SessionNonBlocking()
        {
            m_hRecvBuffer   = new byte[1024];
            m_hMs           = new MemoryStream(m_hRecvBuffer);
            m_hReader       = new BinaryReader(m_hMs);
        }

        public void Update()
        {
            if (m_hEvent != null)
                m_hEvent.Raise();

            if (m_hSocket.Connected)
            {
                try
                {
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
            m_hSocket.BeginConnect(sAddr, iPort, OnEndConnect, m_hSocket);
        }


        private void OnEndConnect(IAsyncResult hRes)
        {
            m_hSocket.EndConnect(hRes);            
            m_hSocket.Blocking = false;
            m_hEvent = new SessionConnectionEvent(this);           
        }

        //TODO: in modalità non di blocco, il metodo Send può risultare completato anche se invia una quantità di byte inferiore al numero di byte presente nel buffer.L'applicazione deve tenere traccia del numero di byte e ritentare l'operazione fino all'invio dei byte nel buffer
        public void Send(Packet hPacket)
        {
            throw new NotImplementedException();
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

        private interface ISessionEvent
        {
            void Raise();
        }

        private class SessionConnectionEvent : ISessionEvent
        {
            private SessionNonBlocking m_hSession;
            public SessionConnectionEvent(SessionNonBlocking hSession)
            {
                m_hSession = hSession;
            }
            public void Raise()
            {
                if (m_hSession.Connected != null)
                {
                    m_hSession.Connected();
                }
            }
        }



    }
}
