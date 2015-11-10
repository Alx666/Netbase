using Netbase.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class RequestCodeGen
    {
        public string           Name            { get; private set; }
        public RpcMethodInfo    RpcInfo         { get; private set; }
        public string           Code            { get; private set; }

        public RequestCodeGen(RpcMethodInfo hRpcInfo, int iProtocolCounter)
        {
            Name        = string.Format("Request{0}", hRpcInfo.Method.Name);
            RpcInfo     = hRpcInfo;

            StringBuilder hPropertyList = new StringBuilder();
            hRpcInfo.Method.GetParameters().ToList().ForEach(hP => hPropertyList.AppendFormat("public {0}\t\t{1}\t\t\t{{ get; private set; }}{2}\t", hP.ParameterType.AsKeyword(), hP.Name, Environment.NewLine));

            StringBuilder hParamList = new StringBuilder();
            hRpcInfo.Method.GetParameters().ToList().ForEach(hP => hParamList.AppendFormat("{0} {1},", hP.ParameterType.AsKeyword(), hP.Name));
            if (hParamList.Length > 0)
                hParamList.Remove(hParamList.Length - 1, 1);

            StringBuilder hEncodeCalls = new StringBuilder();
            hRpcInfo.Method.GetParameters().ToList().ForEach(hP => hEncodeCalls.AppendFormat(@"              m_hWriter.Write({0});{1}", hP.Name, Environment.NewLine));
            if (hEncodeCalls.Length > 0)
                hEncodeCalls.Remove(hEncodeCalls.Length - 1, 1);

            StringBuilder hReadCalls = new StringBuilder();
            hRpcInfo.Method.GetParameters().ToList().ForEach(hP => hReadCalls.AppendFormat(@"            {0} = hReader.Read{1}();{2}", hP.Name, hP.ParameterType.Name, Environment.NewLine));
            if (hReadCalls.Length > 0)
                hReadCalls.Remove(hReadCalls.Length - 1, 1);

            string sResult = string.Format(@"
                using Netbase.Shared;
                using System;
                using System.Collections.Generic;
                using System.IO;
                using System.Text;
                
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
                hRpcInfo.Service.Pair.Client.Namespace,
                iProtocolCounter,
                Name,
                hPropertyList.ToString(),
                Name,
                1024,
                hParamList,
                hEncodeCalls,
                hReadCalls);

            Code = sResult;
        }



    }
}
