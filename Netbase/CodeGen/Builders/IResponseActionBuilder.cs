
namespace Netbase.CodeGen
{
    internal interface IResponseActionBuilder
    {
        string UsingNamespace(RpcService hService);
        string MethodInitialization { get; }
        string MethodInvoke { get; }
        string Namespace(RpcMethodInfo RpcInfo);
    }
}
