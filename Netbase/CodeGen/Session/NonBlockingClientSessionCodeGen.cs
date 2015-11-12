using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class NonBlockingClientSessionCodeGen
    {
        public RpcService   Service     { get; private set; }
        public bool         IsAbstract  { get; private set; }
        public string       Code        { get; private set; }
                       
        public NonBlockingClientSessionCodeGen(RpcService hService)
        {
            Service             = hService;
            IsAbstract          = hService.Pair.Client.Rpcs.Length > 0;

            string sResult = string.Format(@"
            using System;
            using System.Collections.Generic;
            using Netbase.Shared;
            using Netbase.Shared.UI;

            namespace {0}
            {{                
                public{1}class {2} : SessionNonBlocking
                {{
                    private Pool<RpcCall>               m_hRpcPool;
                    private Dictionary<ushort, RpcCall> m_hPendingCalls;
                    private ushort                      m_uCallCounter;
                    
                    {3}

                    public {2}()
                    {{

                    }}              

                    {4}      
                }}
            }}
            ",
            this.Service.Namespace,
            IsAbstract ? " abstract " : " ",
            "NonBlocking" + hService.Name,
            this.PacketRegistration(),
            this.WriteRpcMethods());

            Code = sResult;
        }

        private string WriteRpcMethods()
        {
            StringBuilder hSb = new StringBuilder();
            Service.Pair.Server.Rpcs.Where(hR => hR.Response != null).ToList().ForEach(hM => this.WriteMethod(hM, hSb));
            Service.Pair.Server.Rpcs.Where(hR => hR.Response == null).ToList().ForEach(hM => this.WriteOneWayMethod(hM, hSb));
            Service.Pair.Client.Rpcs.ToList().ForEach(hM => this.WriteSessionEvent(hM, hSb));
            return hSb.ToString();
        }

        private void WriteOneWayMethod(RpcMethodInfo hRpc, StringBuilder hSb)
        {
            string sResult = string.Format(@" 
                    public void {0}({1})
                    {{
                        {3}     hMsg = null;
                        RpcCall hCall = null; 
                                       
                        hMsg        = Interpreter.Get<{3}>();
                        hMsg.Encode({2});      
                        
                        this.Send(hMsg);     
                    }}",
                    hRpc.Method.Name,
                    hRpc.Method.GetParametersString(true),
                    hRpc.Method.GetParametersString(false),
                    hRpc.Request.Name);

            hSb.AppendLine(sResult);
        }

        private void WriteMethod(RpcMethodInfo hRpc, StringBuilder hSb)
        {
            string sReturnType = hRpc.Method.ReturnType.AsKeyword();
            string sReturnCode = sReturnType == "void" ? string.Empty : string.Format("return (hCall.Data as {0}).Data;", hRpc.Response.Name);
            string sInvokeCode = sReturnType == "void" ? string.Empty : string.Format("({0})hResponse.Data", sReturnType);
            string sActionDecl = sReturnType == "void" ? "Action" : string.Format("Action<{0}>", sReturnType);
            string sAsyncFuncArgs = hRpc.Method.GetParametersString(true).Length == 0 ? sActionDecl + " hCallback" : string.Format("{0}, {1} hCallback", hRpc.Method.GetParametersString(true), sActionDecl);

            //TODO: better thread Safety on method calls
            string sResult = string.Format(@" 
                    public void {1}({7})
                    {{
                        {5}     hMsg = null;
                        RpcCall hCall = null; 

                        hCall       = m_hRpcPool.Get();             //Todo: incorporare incremento del contatore nel pool dedicato
                        hCall.Cb    = hCallback;
                        hMsg        = Interpreter.Get<{5}>();
                        hMsg.CallId = m_uCallCounter++;
                        hMsg.Encode({3});      
                        
                        m_hPendingCalls.Add(hMsg.CallId, hCall);
                        
                        this.Send(hMsg);          
                    }}
                    
                    public void On{1}({6} hResponse)
                    {{
                        RpcCall hCall = m_hPendingCalls[hResponse.CallId];
                        m_hPendingCalls.Remove(hResponse.CallId);
   
                        hCall.Data = hResponse;

                        (hCall.Cb as {8}).Invoke({9});
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

        private string PacketRegistration()
        {
            StringBuilder hPacketRegistration = new StringBuilder();

            hPacketRegistration.AppendFormat("static {0}(){1}", "NonBlocking" + Service.Name, Environment.NewLine);
            hPacketRegistration.AppendLine("                    {");

            foreach (RpcMethodInfo hMethod in Service.Pair.Server.Rpcs)
            {
                hPacketRegistration.AppendFormat("                        Interpreter.Register<{0}>();{1}", hMethod.Request.Name, Environment.NewLine);
            }

            foreach (RpcMethodInfo hMethod in Service.Pair.Server.Rpcs.Where(hR => hR.ResponseAction != null))
            {
                hPacketRegistration.AppendFormat("                        Interpreter.RegisterAction<{0}>();{1}", hMethod.ResponseAction.Name, Environment.NewLine);
            }

            foreach (RpcMethodInfo hMethod in Service.Rpcs.Where(hR => hR.Response != null))
            {
                hPacketRegistration.AppendFormat("                        Interpreter.Register<{0}>();{1}", hMethod.Response.Name, Environment.NewLine);
            }

            foreach (RpcMethodInfo hMethod in Service.Rpcs)
            {
                hPacketRegistration.AppendFormat("                        Interpreter.RegisterAction<{0}>();{1}", hMethod.RequestAction.Name, Environment.NewLine);
            }

            hPacketRegistration.AppendLine("                        Interpreter.Warmup(5);");
            hPacketRegistration.AppendLine("                    }");


            return hPacketRegistration.ToString();
        }
    }
}
