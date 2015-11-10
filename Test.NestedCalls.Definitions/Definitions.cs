using Netbase.Shared;

namespace Test.NestedCalls.Definitions
{
    [ServiceContract("NestedCallsService", typeof(INestedCallsSession))]
    public interface INestedCallsService
    {
        [ServiceOperation]
        int SomeMethod(int i);
    }

    [CallbackContract("NestedCallsSession", typeof(INestedCallsService))]
    public interface INestedCallsSession
    {

        [ServiceOperation]
        int SomeCBMethod(int i);
    }
}
