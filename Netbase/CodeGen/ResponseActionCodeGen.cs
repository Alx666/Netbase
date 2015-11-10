using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.CodeGen
{
    internal class ResponseActionCodeGen
    {
        public string           Name    { get; private set; }
        public RpcMethodInfo    RpcInfo { get; private set; }
        public string           Code    { get; private set; }

        public ResponseActionCodeGen(IResponseActionBuilder hBuilder, RpcMethodInfo hRpc)
        {
            Name    = string.Format("Response{0}Action", hRpc.Method.Name);
            RpcInfo = hRpc;

            string sResult = string.Format(@"
                using System;
                using System.Reflection;              
                using Netbase.Shared;       
                {0}
                
                namespace {1}
                {{
                    //{6}
                    public class {2} : {3}, IAction
                    {{                  
                        private const string      m_sMethodName = ""On{5}"";
                        private static MethodInfo m_hMethod;    
                
                        public void Execute(IService hService, ISession hContext)
                        {{
                            if(m_hMethod == null)
                                m_hMethod = hService.GetType().GetMethod(m_sMethodName);
                            
                            {4}
                        }}
                    }}
                }}
                ",
                hBuilder.UsingNamespace(RpcInfo.Service),
                hBuilder.Namespace(RpcInfo),
                Name,
                RpcInfo.Response.Name,
                hBuilder.MethodInvoke,
                RpcInfo.Method.Name,
                "Generated With: " + hBuilder.GetType().Name);

            Code = sResult;
        }
    }
}


            //string sInvoke;
            //string sGetMethod;
            //string sNamespace            = bCallbackService ? sCalleeNamespace : sCallerNamespace;
            //string sAddictionaNamespace  = bCallbackService ? string.Format("using {0};", sCallerNamespace) : string.Empty;

            //if (!bCallbackService)
            //{
            //    sGetMethod  = "m_hMethod = hContex.Service.GetType().GetMethod(m_sMethodName);";
            //    sInvoke     = @"m_hMethod.Invoke(hContex.Service, new object[] { this });";
            //}
            //else
            //{
            //    sGetMethod  = "m_hMethod = hContex.GetType().GetMethod(m_sMethodName);";
            //    sInvoke     = @"m_hMethod.Invoke(hContex, new object[] { this });";
            //}