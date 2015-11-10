using Netbase.Shared;

namespace Test.LongCalls.Definitions
{
    [ServiceContract("LongCallsService", typeof(ILongCallsSession))]
    public interface ILongCallsService
    {
        [ServiceOperation]
        int TimedCall(int iSeconds);
    }

    [CallbackContract("LongCallsSession", typeof(ILongCallsService))]
    public interface ILongCallsSession
    {
        [ServiceOperation]
        int TimedCallCB(int iSeconds);
    }
}
