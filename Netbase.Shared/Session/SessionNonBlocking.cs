using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    public class SessionNonBlocking : ISession, IService, IDisposable
    {
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Recycle(ISession hSession)
        {
            throw new NotImplementedException();
        }

        public void Send(Packet hPacket)
        {
            throw new NotImplementedException();
        }
    }
}
