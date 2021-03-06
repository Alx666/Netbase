﻿using Netbase.Shared;
using Netbase.Shared.UI;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

//https://thoughtstreams.io/glyph/your-game-doesnt-need-udp-yet/
namespace Netbase.Server
{
    internal interface IServerIOCP : IService
    {
        void Recycle(SessionIOCP hSession);
    }



    public class ServerIOCP<C> : IServerIOCP, IDisposable where C : SessionIOCP, new()        
    {        
        public delegate void ConnectionEventHandler(C hContext);
        public delegate void DisconnectionEventHandler(C hContext, Exception hEx, SocketError eErr);

        private const int                           BackLog         = 50;                   //To XML
        private const int                           MaxPacketSize   = 1024;                 //To XML
        private ConcurrentPool<C>                   m_hPool;
        private Socket                              m_hListener;                            //To Multiple Acceptors
        private SocketAsyncEventArgs                m_hAcceptOp;                            //To Multiple Acceptors
        private object                              m_hAcceptSyncRoot;                      //To Multiple Acceptors
        private AutoResetEvent                      m_hExitEvent;

        protected Interpreter                       Interpreter { get; private set; }
        protected ConcurrentDictionary<ushort, C>   m_hClients;                             //To make private, better accessor logic required

        public event ConnectionEventHandler         ClientConnected;
        public event DisconnectionEventHandler      ClientDisconnected;        
        public event EventHandler                   ListenerError;                          //To Dedicated Error Hanlder
        
        public ServerIOCP()
        {
            m_hClients        = new ConcurrentDictionary<ushort, C>();
            m_hPool           = new ConcurrentPool<C>();            
            m_hAcceptSyncRoot = new object();
            Interpreter       = new Interpreter(new ConcurrentPacketPoolAllocator());
            m_hExitEvent      = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            this.Stop();
        }

        public virtual void Start(IPAddress hAddr, int iPort)
        {
            lock (m_hAcceptSyncRoot)
            {
                if (m_hListener != null)
                    throw new ApplicationException(string.Format("Server Already Running ({0})", m_hListener.LocalEndPoint));

                m_hListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_hListener.Bind(new IPEndPoint(hAddr, iPort));
                m_hListener.Listen(BackLog);

                //Can't setup a buffer for AcceptOp cause it will make the callback wait for the initial ammount of data causing lag on the accept routine
                m_hAcceptOp             = new SocketAsyncEventArgs();
                m_hAcceptOp.Completed  += this.AcceptAsyncCompleted;
                m_hAcceptOp.UserToken   = this;

                this.StartAccept(m_hAcceptOp);
            }            
        }

        private void StartAccept(SocketAsyncEventArgs hE)
        {
            if (!m_hListener.AcceptAsync(hE))
                this.AcceptAsyncCompleted(this, hE);
        }

        public virtual void Stop()
        {
            lock (m_hAcceptSyncRoot)
            {
                if (m_hListener == null)
                    return;

                m_hListener.Dispose();
                m_hAcceptOp.Dispose();
                m_hListener = null;
                m_hAcceptOp = null;
            }

            m_hExitEvent.Set();
        }

        private void AcceptAsyncCompleted(object hSender, SocketAsyncEventArgs hE)
        {
            if (m_hAcceptOp.SocketError != SocketError.Success && ListenerError != null)
                ListenerError(this, null);

            Socket hSocket              = m_hAcceptOp.AcceptSocket;
            m_hAcceptOp.AcceptSocket    = null;  //Important! Prevent Socket Recycling
            this.StartAccept(hE);

            C hSession             = m_hPool.Get();
            hSession.Id            = IdGenerator.Get();
            hSession.Socket        = hSocket;            
            hSession.Service       = this;
            hSession.ServingMode   = new ServingModeParallel(this, hSession);
            hSession.Interpreter   = Interpreter;

            if (m_hClients.TryAdd(hSession.Id, hSession))
            {
                hSession.Start();

                if (ClientConnected != null)
                    ClientConnected(hSession);
            }
            else
            {
                //TODO Error Handling Implement after pooling logic
                throw new NotImplementedException();
            }
        }


        public void Recycle(SessionIOCP hSession)
        {            
            C hRemoved;
            if (m_hClients.TryRemove(hSession.Id, out hRemoved))
            {
                hRemoved.Close();
                IdGenerator.Recycle(hRemoved.Id);
                m_hPool.Recycle(hSession as C);

                if (ClientDisconnected != null)
                    ClientDisconnected(hSession as C, null, SocketError.Success);
            }
            else
            {
                if (ClientDisconnected != null)
                    ClientDisconnected(hSession as C, null, SocketError.Success);
            }                                     
        }

        
        public void WaitForExit()
        {
            m_hExitEvent.WaitOne();
        }



        public void DispatchExclude(Packet hPacket, C hClient)
        {
            
        }

        [ConsoleUIMethod]
        public string DiagnosticStatus()
        {
            if (m_hListener != null)
                return string.Format("Server Running: {0}", m_hListener.LocalEndPoint);
            else
                return "Server offline";
        }


    }
}
