using System;
using System.IO;
using System.Net.Sockets;
using Netbase.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Netbase.Server
{
    //xxxx
    public abstract class SessionIOCP : ISession, IDisposable
    {
        private const int BufferSize = 1024;        
                    
        public Socket           Socket                  { get; set; }
        public ushort           Id                      { get; set; }
        public Interpreter      Interpreter             { get; set; }

        internal IServerIOCP    Service                 { get; set; }
        internal IServingMode   ServingMode             { get; set; }

        private ConcurrentPool<SocketAsyncEventArgs> m_hSendOps;
        private ConcurrentPool<SocketAsyncEventArgs> m_hRecvOps;

        private byte[]                               m_hRecvBuffer;
        private int                                  m_iCurrentOffset;
        private int                                  m_iToDataToConsume;
        private MemoryStream                         m_hMs;
        private BinaryReader                         m_hReader;
       

        public SessionIOCP()
        {
            //TODO: optimize SocketAsyncEventArgs distribution
            m_hSendOps = new ConcurrentPool<SocketAsyncEventArgs>();
            m_hRecvOps = new ConcurrentPool<SocketAsyncEventArgs>();            

            for (int i = 0; i < 10; i++)
            {
                SocketAsyncEventArgs hSendOp = new SocketAsyncEventArgs();
                hSendOp.Completed += OnSendCompleted;
                hSendOp.SetBuffer(new byte[BufferSize], 0, BufferSize);
                m_hSendOps.Recycle(hSendOp);

                SocketAsyncEventArgs hRecvOp = new SocketAsyncEventArgs();
                hRecvOp.Completed += OnRecvCompleted;
                hRecvOp.SetBuffer(new byte[BufferSize], 0, BufferSize);
                m_hRecvOps.Recycle(hRecvOp);
            }


            m_hRecvBuffer   = new byte[BufferSize];
            m_hMs           = new MemoryStream(m_hRecvBuffer);
            m_hReader       = new BinaryReader(m_hMs);
        }

        public void Dispose()
        {             
            //Todo: dispose pools
            //m_hSendOp.Dispose();
            //m_hRecvOp.Dispose();
        }

        public void Close()
        {
            try
            {
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Close();
                this.Socket = null;
            }
            catch (Exception)
            {
                //TODO: better check if something went wrong during shutdown
            }
        }

        public void Start()
        {
            this.StartRecv();
        }

        private void StartRecv()
        {
            SocketAsyncEventArgs hRecv = m_hRecvOps.Get();
            if (!Socket.ReceiveAsync(hRecv))
                OnRecvCompleted(this, hRecv);
        }

        private void OnRecvCompleted(object hSender, SocketAsyncEventArgs hE)
        {                        
            try
            {
                if (hE.IsDisconnect())
                {
                    m_hRecvOps.Recycle(hE);
                    throw new SocketException();
                }

                //Begin another Receive Operation
                m_iToDataToConsume += hE.CopyTo(m_hRecvBuffer, ref m_iCurrentOffset); //Warning: race condition with next call
                
                m_hRecvOps.Recycle(hE);
                                
                byte    bId;
                ushort  uDataSize;
                int     iPacketIndex = 0;
                
                while (m_hRecvBuffer.ContainsPacket(iPacketIndex, m_iCurrentOffset, m_iToDataToConsume, out bId, out uDataSize))
                {
                    m_hMs.Seek(iPacketIndex + Packet.DataSizeIndex, SeekOrigin.Begin);

                    IAction hAction = Interpreter.Get(bId);

                    hAction.LoadData(m_hReader);                    

                    iPacketIndex        += Packet.HeaderSize + uDataSize;
                    m_iToDataToConsume  -= Packet.HeaderSize + uDataSize;

                    ServingMode.Execute(hAction);             
                      
                    m_hRecvBuffer.Reorder(iPacketIndex, ref m_iCurrentOffset);
                }

                this.StartRecv();
            }
            catch (Exception)
            {                
                Service.Recycle(this);
            }
        }
        
        public void Send(Packet hPacket)
        {            
            try
            {
                SocketAsyncEventArgs hSendOp = m_hSendOps.Get();

                Buffer.BlockCopy(hPacket.Buffer, 0, hSendOp.Buffer, 0, Packet.HeaderSize + hPacket.DataSize);
                hSendOp.SetBuffer(0, Packet.HeaderSize + hPacket.DataSize);

                if (!Socket.SendAsync(hSendOp))
                    this.OnSendCompleted(this, hSendOp);                
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx);
            }
        }

        private void OnSendCompleted(object hSender, SocketAsyncEventArgs hE)
        {
            m_hSendOps.Recycle(hE);
        } 
    }

    public static class SomeExtensions
    {
        public static bool IsDisconnect(this SocketAsyncEventArgs hThis)
        {
            if (hThis.BytesTransferred == 0 || hThis.SocketError != SocketError.Success)
                return true;
            else
                return false;
        }

        public static int CopyTo(this SocketAsyncEventArgs hThis, byte[] hBuffer, ref int iOffset)
        {
            Buffer.BlockCopy(hThis.Buffer, 0, hBuffer, iOffset, hThis.BytesTransferred);
            iOffset += hThis.BytesTransferred;
            return hThis.BytesTransferred;
        }

        public static bool ContainsPacket(this byte[] hThis, int iOffset, int iBufferDataOffset, int iDataToConsume, out byte bId, out ushort uDataSize)
        {            
            bId         = hThis[iOffset];
            uDataSize   = BitConverter.ToUInt16(hThis, iOffset + 1);

            if (Packet.HeaderSize + uDataSize <= iBufferDataOffset && iDataToConsume > 0)
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Reorder(this byte[] hThis, int iCurrentIndex, ref int iDataOffset)
        {
            Buffer.BlockCopy(hThis, iCurrentIndex, hThis, 0, iDataOffset - iCurrentIndex);
            Array.Clear(hThis, iDataOffset - iCurrentIndex, iDataOffset);
            iDataOffset = 0;
        }
    }
}