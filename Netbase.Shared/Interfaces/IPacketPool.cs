using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    public interface IPool
    {
        Packet Get();

        void Recycle(Packet hPacket);
    }
}
