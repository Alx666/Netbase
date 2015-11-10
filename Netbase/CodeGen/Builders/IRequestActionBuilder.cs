namespace Netbase.CodeGen
{
    internal interface IRequestActionBuilder
    {
        string UsingNamespace(RpcService hService);

        string ResponseDeclaration(ResponseCodeGen hResponse);

        string ResponseAllocation(ResponseCodeGen hResponse);

        string ResponseCallIdSet { get; }

        string ResponseSend { get; }

        string MethodCall(RpcMethodInfo hMethod);

        string MethodToCall(RpcMethodInfo RpcInfo);
    }
}
