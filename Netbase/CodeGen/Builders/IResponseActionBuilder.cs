
namespace Netbase.CodeGen
{
    internal interface IResponseActionBuilder
    {
        string UsingNamespace(RpcService hService);
        string MethodInvoke { get; }
        string Namespace(RpcMethodInfo RpcInfo);
    }
}
