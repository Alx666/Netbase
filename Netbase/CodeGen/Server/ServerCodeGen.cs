using Netbase.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class ServerCodeGen
    {
        public string Name { get; private set; }
        public string Code { get; private set; }

        public ServerCodeGen(RpcService hService)
        {
            Name            = hService.Name;

            StringBuilder hSb = new StringBuilder();

            foreach (RpcMethodInfo hMethod in hService.Rpcs)
            {
                StringBuilder hParamList = new StringBuilder();

                hParamList.AppendFormat("T hContext, ");

                foreach (ParameterInfo hParam in hMethod.Method.GetParameters())
                {
                    hParamList.AppendFormat("{0} {1}, ", hParam.ParameterType.AsKeyword(), hParam.Name);
                }

                hParamList.Remove(hParamList.Length - 2, 2);

                hSb.AppendFormat("      public abstract {0} {1}({2});{3}", hMethod.Method.ReturnType.AsKeyword(), hMethod.Method.Name, hParamList, Environment.NewLine);
            }


            StringBuilder hCb = new StringBuilder();
            hService.Pair.Client.Rpcs.ToList().ForEach(hM => this.WriteServerCallbackMethod(hM, hCb));



            StringBuilder hPacketRegistration = new StringBuilder();

            foreach (RpcMethodInfo hMethod in hService.Rpcs)
            {
                hPacketRegistration.AppendFormat("                            Interpreter.RegisterAction<{0}>();{1}", hMethod.RequestAction.Name, Environment.NewLine);
            }

            foreach (RpcMethodInfo hMethod in hService.Pair.Client.Rpcs.Where(hR => hR.ResponseAction != null))
            {
                hPacketRegistration.AppendFormat("                            Interpreter.RegisterAction<{0}>();{1}", hMethod.ResponseAction.Name, Environment.NewLine);
            }

            foreach (RpcMethodInfo hMethod in hService.Pair.Client.Rpcs)
            {
                hPacketRegistration.AppendFormat("                            Interpreter.Register<{0}>();{1}", hMethod.Request.Name, Environment.NewLine);
            }

            hPacketRegistration.AppendLine("                            Interpreter.Warmup(5);");

            Code = string.Format(@"
                using System;
                using Netbase.Shared;
                using Netbase.Server;
                using {4};

                namespace {0}
                {{
                    public abstract class {1}<T> : ServerIOCP<T> where T: SessionIOCP, new()
                    {{
                        public {1}()
                        {{
{3}
                        }}

                        {2}                        
                    }}
                }}
                ",
                hService.Namespace,
                Name,
                hSb.ToString(),
                hPacketRegistration,
                hService.Pair.Client.Namespace);
        }

        private void WriteServerCallbackMethod(RpcMethodInfo hTransportCodeGen, StringBuilder hSb)
        {
            string sResult = string.Format(@"
            public abstract void On{0}(T hContex, {1} hData);
            ",
            hTransportCodeGen.Method.Name,
            "Response" + hTransportCodeGen.Method.Name);


            hSb.AppendLine(sResult);
        }
    }
}
