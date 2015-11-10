using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Netbase.Shared
{
    public class Session : ISession, IService, IDisposable
    {                                       
        private Socket              m_hSocket;
        private Thread              m_hClientThread;
        private byte[]              m_hDataBuffer;
        private MemoryStream        m_hReader;
        private BinaryReader        m_hSr;
        private volatile bool       m_bDisposed;
        
        protected static Interpreter  Interpreter;

        static Session()
        {
            Interpreter     = new Interpreter(); 
        }

        public Session()
        {
            m_hClientThread = new Thread(ThreadRoutine);
            m_hDataBuffer   = new byte[1024];
            m_hReader       = new MemoryStream(m_hDataBuffer);
            m_hSr           = new BinaryReader(m_hReader);            
        }
             

        #region IDisposable

        //Finalizer
        ~Session()
        {
            this.Dispose(false);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }
        
        protected virtual void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                if (!m_bDisposed)
                {
                    m_bDisposed = true;
                    m_hSr.Close();
                    m_hReader.Close();
                    this.m_hSocket.Shutdown(SocketShutdown.Both);
                    this.m_hSocket.Close();

                    if(m_hClientThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                        m_hClientThread.Join();
                }
            }
        }

        #endregion

        public void Connect(string sAddr, int iPort)
        {
            m_hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.m_hSocket.Connect(sAddr, iPort); 
            this.Start();
        }

        private void ThreadRoutine()
        {
            while(true)
            {
                IAction hAction = null;

                try
                {                   
                    //Estrazione e ricostruzione di un pacchetto dalla rete, questo è richiesto solo per il TCP
                    byte bPacketId = Session.ReceiveAll(this.m_hSocket, m_hDataBuffer);

                    //Portiamo lo stream in posizione per leggere i dati
                    m_hReader.Seek(Packet.DataSizeIndex, SeekOrigin.Begin);

                    //Dobbiamo implementare il meccanismo per prendere il pacchetto giusto in base all'id ricevuto
                    hAction = Interpreter.Get(bPacketId);

                    //Carichiamo la action
                    hAction.LoadData(m_hSr);

                    //Eseguiamo l'action                    
                    //hAction.Execute(this);
                    ThreadPool.QueueUserWorkItem(Execution, hAction); 
                }
                catch (Exception)
                {
                    //TODO: evento disconnessione clientside
                    this.Dispose();
                    break;
                }
                finally
                {
                    //if (hAction != null)
                    //    Interpreter.Recycle(hAction as Packet);
                }
            }
        }

        private void Execution(object hState)
        {
            try
            {
                (hState as IAction).Execute(this.Service, this);
            }
            finally
            {
                //if (hState != null)
                //    Interpreter.Recycle(hState as Packet);
            }
        }

        private void Start()
        {
            m_hClientThread.Start();
        }

        public void Send(Packet hPacket)
        {
            m_hSocket.Send(hPacket.Buffer, hPacket.DataSize + Packet.HeaderSize, SocketFlags.None);
        }

        private static byte ReceiveAll(Socket hSocket, byte[] hDataBuffer)
        {
            SocketError eError; //Ogni operazione andata a buon fine termna con SocketError.Success
            int iReceived;      //Se iReceived == 0, la connessione è stata chiusa correttamente dal peer

            //Ricezione Header
            iReceived = hSocket.Receive(hDataBuffer, 0, Packet.HeaderSize, SocketFlags.None, out eError);
            if (iReceived == 0 || eError != SocketError.Success)
                throw new SocketException();

            ushort uSize    = BitConverter.ToUInt16(hDataBuffer, 1);
            int iOffet      = Packet.HeaderSize;
            int iTotalRecv = 0;

            while (iTotalRecv < uSize)
            {
                iReceived = hSocket.Receive(hDataBuffer, iOffet, uSize - iTotalRecv, SocketFlags.None, out eError);
                if (iReceived == 0 || eError != SocketError.Success)
                    throw new SocketException();

                iTotalRecv += iReceived;
                iOffet += iReceived;
            }

            return hDataBuffer[0];
        }

        //Todo: Hide
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
            get { return this; }
        }

        //TODO: Hide
        public void Recycle(ISession hSession)
        {
            throw new NotImplementedException();
        }
    }
}