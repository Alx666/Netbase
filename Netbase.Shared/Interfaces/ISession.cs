namespace Netbase.Shared
{
    public interface ISession
    {
        ushort Id { get; set; }
        void Send(Packet hPacket);
        IService Service { get; }
    }
}
