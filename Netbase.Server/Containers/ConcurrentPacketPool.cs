using Netbase.Shared;

namespace Netbase.Server
{
    internal class ConcurrentPacketPool<T> : ConcurrentPool<T>, IPool
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

    internal class ConcurrentPacketPoolAllocator : Interpreter.IPoolAllocator
    {
        public IPool Get<T>() where T : Packet, new()
        {
            return new ConcurrentPacketPool<T>();
        }
    }
}
