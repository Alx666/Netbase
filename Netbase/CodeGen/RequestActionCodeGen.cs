using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.CodeGen
{
    internal class RequestActionCodeGen
    {
        public string           Name    { get; private set; }
        public RpcMethodInfo    RpcInfo { get; private set; }
        public string           Code    { get; private set; }



        public RequestActionCodeGen(IRequestActionBuilder hBuilder, RpcMethodInfo hRpc)
        {

            Name    = string.Format("Request{0}Action", hRpc.Method.Name);
            RpcInfo = hRpc;


            string sResult = string.Format(@"                        
                using System;
                using System.Reflection;                        
                using Netbase.Shared;
                {0}
                
                namespace {1}
                {{
                    //{10}
                    public class {2} : {3}, IAction
                    {{             
                        private const string      m_sMethodName = ""{4}"";
                        private static MethodInfo m_hMethod;    
                        {5}
                
                        public {2}() : base()
                        {{          
                            {6}
                        }}
                
                        public void Execute(ISession hContext)
                        {{
                            if(m_hMethod == null)
                                m_hMethod = hContext.Service.GetType().GetMethod(m_sMethodName);
                
                            
                            {7}
                
                            {8}                                             

                            {9}
                        }}
                    }}
                }}
                ",
                hBuilder.UsingNamespace(RpcInfo.Service),            //0 - using ProjName.Service;
                RpcInfo.Service.Namespace,                           //1
                Name,                                                //2
                RpcInfo.Request.Name,                                //3
                hBuilder.MethodToCall(RpcInfo),                                 //4
                hBuilder.ResponseDeclaration(RpcInfo.Response),      //5 - private ResponseSomeThing m_hResponse
                hBuilder.ResponseAllocation(RpcInfo.Response),       //6 - m_hResponse = new ResponseSomeThing();
                hBuilder.ResponseCallIdSet,                          //7 - m_hResponse.CallId = this.CallId;
                hBuilder.MethodCall(RpcInfo),
                hBuilder.ResponseSend,                               //8 - hContext.Send(m_hResponse);
                "Generated With: " + hBuilder.GetType().Name
                );

            Code = sResult;
        } 
    }
}

//bool    bIsVoid      = hMethod.ReturnType.AsKeyword() == "void";
//string  sParamList   = hMethod.GetParametersString(false);
//int     sParamCount  = sParamList.Split(new char[] { ',' }).Length;
//string  sContex      = bCallbackService ? string.Empty : "hContex,";
//string  sMethodCall  = string.Format("m_hMethod.Invoke(hContex.Service, new object[] {{ {0} {1} }})", sContex, sParamList);
//string  sNamespace   = bCallbackService ? sCallerNamespace : sCalleeNamespace;
//string sAddictionaNamespace = !bCallbackService ? string.Format("using {0};", sCallerNamespace) : string.Empty;

//if (bIsVoid)
//{
//    sMethodCall = string.Format("{0}; m_hResponse.Encode();", sMethodCall);
//}
//else
//{
//    sMethodCall = string.Format("m_hResponse.Encode(({0}){1});", hMethod.ReturnType.AsKeyword(), sMethodCall);
//}