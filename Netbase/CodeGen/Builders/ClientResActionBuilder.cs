using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.CodeGen
{
    internal class ClientResActionBuilder : IResponseActionBuilder
    {
        public string UsingNamespace(RpcService hService)
        {
            return string.Format("using {0};", hService.Pair.Client.Namespace);
        }

        public string MethodInvoke
        {
            get { return @"m_hMethod.Invoke(hContext, new object[] { this });"; }
        }

        public string MethodInit
        {
            get { return "m_hMethod = hContext.GetType().GetMethod(m_sMethodName);"; }
        }

        public string Namespace(RpcMethodInfo RpcInfo)
        {
            return RpcInfo.Service.Pair.Server.Namespace;
        }
    }
}
