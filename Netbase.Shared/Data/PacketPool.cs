namespace Netbase.Shared
{
    internal class PacketPool<T> : Pool<T>, IPool
        where T : Packet, new()
    {
        public new Packet Get()
        {
            Packet hPacket  = base.Get();
            hPacket.Pool    = this;
            return hPacket;
        }

        public void Recycle(Packet hPacket)
        {
            base.Recycle(hPacket as T);
        }
    }
}
