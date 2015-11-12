using Netbase.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.CodeGen
{
    internal abstract class SessionCodeGen
    {
        public RpcService Service       { get; private set; }
        public bool IsAbstract          { get; private set; }
        public string Code              { get; private set; }

        protected abstract string FirstUsingDirective();
        protected abstract string SecondUsingDirective();
        protected abstract string BaseType();
        protected abstract string PacketRegistration();
        protected abstract string Name();
        protected abstract string WriteRpcMethods();
        

        public SessionCodeGen(RpcService hService)
        {
            Service             = hService;
            IsAbstract          = hService.Pair.Client.Rpcs.Length > 0;
            bool bServerCode    = hService != hService.Pair.Client;
            
            string sResult = string.Format(@"
            using System;
            using System.Collections.Generic;
            using Netbase.Shared;
            using Netbase.Shared.UI;
            {0}
            {1}

            namespace {2}
            {{                
                public{3}class {4} : {5}
                {{
                    private Pool<RpcCall>               m_hRpcPool;
                    private Dictionary<ushort, RpcCall> m_hPendingCalls;
                    private ushort                      m_uCallCounter;
                    
                    {6}

                    public {4}()
                    {{
                        m_hRpcPool      = new Pool<RpcCall>();
                        m_hPendingCalls = new Dictionary<ushort, RpcCall>();
                    }}

                    {7}
                    
                }}
            }}
            ",
            this.FirstUsingDirective(),
            this.SecondUsingDirective(),
            this.Service.Namespace,
            IsAbstract ? " abstract " : " ",
            this.Name(),
            this.BaseType(),
            this.PacketRegistration(),
            this.WriteRpcMethods());

            Code = sResult;
        }

        protected void WriteOneWayMethod(RpcMethodInfo hRpc, StringBuilder hSb)
        {
            string sResult = string.Format(@" 
                    public void {0}({1})
                    {{
                        {3}     hMsg = null;
                        RpcCall hCall = null; 
                                            
                        try
                        {{
                            hMsg        = Interpreter.Get<{3}>();
                            hMsg.Encode({2});      

                            this.Send(hMsg); 
                        }}
                        finally
                        {{
                            Interpreter.Recycle(hMsg);   
                            m_hRpcPool.Recycle(hCall);
                        }}
                    }}",
                    hRpc.Method.Name,
                    hRpc.Method.GetParametersString(true),
                    hRpc.Method.GetParametersString(false),
                    hRpc.Request.Name);

            hSb.AppendLine(sResult);
        }

        protected void WriteMethod(RpcMethodInfo hRpc, StringBuilder hSb)
        {
            string sReturnType      = hRpc.Method.ReturnType.AsKeyword();
            string sReturnCode      = sReturnType == "void" ? string.Empty : string.Format("return (hCall.Data as {0}).Data;", hRpc.Response.Name);
            string sInvokeCode      = sReturnType == "void" ? string.Empty : string.Format("({0})hResponse.Data", sReturnType);
            string sActionDecl      = sReturnType == "void" ? "Action"     : string.Format("Action<{0}>", sReturnType);
            string sAsyncFuncArgs   = hRpc.Method.GetParametersString(true).Length == 0 ? sActionDecl + " hCallback" : string.Format("{0}, {1} hCallback", hRpc.Method.GetParametersString(true), sActionDecl);

            //TODO: better thread Safety on method calls
            string sResult = string.Format(@" 
                    public {0} {1}({2})
                    {{
                        {5}     hMsg = null;
                        RpcCall hCall = null; 
                                            
                        try
                        {{
                            hCall       = m_hRpcPool.Get();             //Todo: incorporare incremento del contatore nel pool dedicato
                            hCall.Async = false;
                            hMsg        = Interpreter.Get<{5}>();
                            hMsg.CallId = m_uCallCounter++;
                            hMsg.Encode({3});      

                            lock(m_hPendingCalls)
                            {{
                                m_hPendingCalls.Add(hMsg.CallId, hCall);
                            }}

                            this.Send(hMsg); 

                            hCall.Wait();          
                            {4}             
                        }}
                        finally
                        {{
                            Interpreter.Recycle(hMsg);   
                            m_hRpcPool.Recycle(hCall);
                        }}
                    }}

                    public void {1}({7})
                    {{
                        {5}     hMsg = null;
                        RpcCall hCall = null; 

                        try
                        {{
                            hCall       = m_hRpcPool.Get();             //Todo: incorporare incremento del contatore nel pool dedicato
                            hCall.Async = true;
                            hCall.Cb    = hCallback;
                            hMsg        = Interpreter.Get<{5}>();
                            hMsg.CallId = m_uCallCounter++;
                            hMsg.Encode({3});      

                            lock(m_hPendingCalls)
                            {{
                                m_hPendingCalls.Add(hMsg.CallId, hCall);
                            }}

                            //TODO: make use of an async send operation, to implement on base session object
                            this.Send(hMsg);          
                        }}
                        finally
                        {{
                            Interpreter.Recycle(hMsg);   
                            m_hRpcPool.Recycle(hCall); //TODO: possible error?
                        }}
                    }}
                    
                    public void On{1}({6} hResponse)
                    {{
                        RpcCall hCall;

                        lock(m_hPendingCalls)
                        {{
                            hCall = m_hPendingCalls[hResponse.CallId];
                            m_hPendingCalls.Remove(hResponse.CallId);
                        }}

                        hCall.Data = hResponse;

                        if(hCall.Async)
                        {{
                            (hCall.Cb as {8}).Invoke({9});
                        }}
                        else
                        {{                            
                            hCall.Set();
                        }}

                        m_hRpcPool.Recycle(hCall);
                    }}",
                    sReturnType,
                    hRpc.Method.Name,
                    hRpc.Method.GetParametersString(true),
                    hRpc.Method.GetParametersString(false),
                    sReturnCode,
                    hRpc.Request.Name,
                    hRpc.Response.Name,
                    sAsyncFuncArgs,
                    sActionDecl,
                    sInvokeCode
                    );

            hSb.AppendLine(sResult);
        }

        protected void WriteSessionEvent(RpcMethodInfo hRpc, StringBuilder hSb)
        {
            string sResult = string.Format(@"
            public abstract {0} On{1}({2});
            ",
            hRpc.Method.ReturnType.AsKeyword(),
            hRpc.Method.Name,
            hRpc.Method.GetParametersString(true));

            hSb.AppendLine(sResult);
        }
    }
}
