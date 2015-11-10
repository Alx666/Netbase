using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    public class SessionAsync : ISession, IService, IDisposable
    {
        public SessionAsync()
        {

        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Connect(string sAddr, int iPort, Action hCb)
        { 
            //do stuff

            hCb.Invoke();
        }

        public void Disconnect()
        { 
        }

        public void Recycle(ISession hSession)
        {

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

        public void Send(Packet hPacket)
        {
            throw new NotImplementedException();
        }

        public IService Service
        {
            get { throw new NotImplementedException(); }
        }
    }
}
