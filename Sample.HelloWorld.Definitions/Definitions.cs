using Netbase.Shared;

namespace Sample.HelloWorld.Definitions
{
    [ServiceContract("HelloWorldService", typeof(IHelloWorldSession))]
    public interface IHelloWorldService
    {
        [ServiceOperation]
        string GetMessageFromServer();
    }

    [CallbackContract("HelloWorldSession", typeof(IHelloWorldService))]
    public interface IHelloWorldSession
    {
    }
}
