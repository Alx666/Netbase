using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.CodeGen
{
    internal class ServerResActionBuilder : IResponseActionBuilder
    {
        public string UsingNamespace(RpcService hService)
        {
            return string.Format("using {0};", hService.Pair.Client.Namespace);
        }

        public string MethodInitialization
        {
            get { return "m_hMethod = hContext.Service.GetType().GetMethod(m_sMethodName);"; }
        }

        public string MethodInvoke
        {
            get { return @"m_hMethod.Invoke(hContext.Service, new object[] { this });"; }
        }

        public string Namespace(RpcMethodInfo hRpcInfo)
        {
            return string.Format("{0}", hRpcInfo.Service.Pair.Client.Namespace);
        }
    }
}
