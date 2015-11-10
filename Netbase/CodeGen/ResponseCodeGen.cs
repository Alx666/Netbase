using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class ResponseCodeGen
    {       
        public string           Name            { get; private set; }
        public RpcMethodInfo    RpcInfo         { get; private set; }
        public string           Code            { get; private set; }

        public ResponseCodeGen(RpcMethodInfo hRpcInfo, int iProtocolCounter)
        {
            Name    = string.Format("Response{0}", hRpcInfo.Method.Name);
            RpcInfo = hRpcInfo;

            string sReturnProperty  = hRpcInfo.Method.ReturnType != typeof(void) ? string.Format("public {0} Data;", hRpcInfo.Method.ReturnType.AsKeyword()) : string.Empty;
            string sEncodeParam     = hRpcInfo.Method.ReturnType != typeof(void) ? string.Format("{0} hData", hRpcInfo.Method.ReturnType.AsKeyword()) : string.Empty;
            string sWriteCalls      = hRpcInfo.Method.ReturnType != typeof(void) ? "m_hWriter.Write(hData);" : string.Empty;
            string sLoadCalls       = hRpcInfo.Method.ReturnType != typeof(void) ? string.Format("Data = hReader.Read{0}();", hRpcInfo.Method.ReturnType.Name) : string.Empty;

            string sResult = string.Format(@"
                using System;
                using System.Collections.Generic;
                using System.IO;
                using Netbase.Shared;
                
                namespace {0}
                {{
                    [NetbasePacket({1})]
                    public class {2} : Packet
                    {{
                        {3}
                
                        public {4}() : base({1}, {5})
                        {{
                            
                        }}
                
                        public void Encode({6})
                        {{
                            this.BeginEncode();
                            {7}
                            this.EndEncode();
                        }}
                
                        public override void LoadData(BinaryReader hReader)
                        {{
                            base.LoadData(hReader);
                            {8}
                        }}
                    }}
                }}",
                RpcInfo.Service.Pair.Client.Namespace,
                iProtocolCounter,
                Name,
                sReturnProperty,
                Name,
                1024,
                sEncodeParam,
                sWriteCalls,
                sLoadCalls);

            Code = sResult;
        }
    }
}
