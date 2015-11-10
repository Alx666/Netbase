using System;

namespace Netbase.CodeGen
{
    internal class OneWayServerReqActionBuilder : IRequestActionBuilder
    {
        public string UsingNamespace(RpcService hService)
        {
            return string.Format("using {0};", hService.Pair.Client.Namespace);
        }

        public string ResponseDeclaration(ResponseCodeGen hResponse)
        {
            return string.Empty;
        }

        public string ResponseAllocation(ResponseCodeGen hResponse)
        {
            return string.Empty;
        }

        public string ResponseCallIdSet
        {
            get { return string.Empty; }
        }

        public string ResponseSend
        {
            get { return string.Empty; }
        }


        public string MethodCall(RpcMethodInfo hRpc)
        {
            return string.Format("m_hMethod.Invoke(hService, new object[] {{ hContext, {0} }});", hRpc.Method.GetParametersString(false));
        }


        public string MethodToCall(RpcMethodInfo hRpcInfo)
        {
            return string.Format("{0}", hRpcInfo.Method.Name);
        }
    }     
}
