
namespace Netbase.CodeGen
{
    internal class TwoWayServerReqActionBuilder : IRequestActionBuilder
    {
        public string UsingNamespace(RpcService hService)
        {
            return string.Format("using {0};", hService.Pair.Client.Namespace);
        }

        public string ResponseDeclaration(ResponseCodeGen hResponse)
        {
            return string.Format("private {0} m_hResponse;", hResponse.Name);
        }

        public string ResponseAllocation(ResponseCodeGen hResponse)
        {
            return string.Format("m_hResponse = new {0}();", hResponse.Name);
        }

        public string ResponseCallIdSet
        {
            get { return "m_hResponse.CallId = this.CallId;"; }
        }

        public string ResponseSend
        {
            get { return "hContext.Send(m_hResponse);"; }
        }


        public string MethodCall(RpcMethodInfo hRpc)
        {
            string sMethodCall = string.Format("m_hMethod.Invoke(hService, new object[] {{ hContext, {0} }})", hRpc.Method.GetParametersString(false));

            if (hRpc.Method.ReturnType == typeof(void))
            {
                sMethodCall = string.Format("{0}; m_hResponse.Encode();", sMethodCall);
            }
            else
            {
                sMethodCall = string.Format("m_hResponse.Encode(({0}){1});", hRpc.Method.ReturnType.AsKeyword(), sMethodCall);
            }

            return sMethodCall;
        }


        public string MethodToCall(RpcMethodInfo hRpcInfo)
        {
            return string.Format("{0}", hRpcInfo.Method.Name);
        }
    }  
}
