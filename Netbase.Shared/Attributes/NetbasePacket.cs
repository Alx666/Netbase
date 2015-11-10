using System;

namespace Netbase.Shared
{
    public class NetbasePacket : Attribute
    {
        public byte Id { get; private set; }
        public NetbasePacket(byte bId)
        {
            Id = bId;
        }
    }
}
