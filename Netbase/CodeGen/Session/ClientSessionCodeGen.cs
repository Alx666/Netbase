using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class ClientSessionCodeGen : SessionCodeGen
    {
        public ClientSessionCodeGen(RpcService hService)
            : base(hService)
        {

        }

        protected override string FirstUsingDirective()
        {
            return string.Empty;
        }

        protected override string SecondUsingDirective()
        {
            return string.Empty;
        }

        protected override string BaseType()
        {
            return "Session";
        }

        protected override string PacketRegistration()
        {
            StringBuilder hPacketRegistration = new StringBuilder();

            hPacketRegistration.AppendFormat("static {0}(){1}", Service.Name, Environment.NewLine);
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

        protected override string Name()
        {
            return Service.Name;
        }

        protected override string WriteRpcMethods()
        {
            StringBuilder hSb = new StringBuilder();
            Service.Pair.Server.Rpcs.Where(hR => hR.Response != null).ToList().ForEach(hM => this.WriteMethod(hM, hSb));
            Service.Pair.Server.Rpcs.Where(hR => hR.Response == null).ToList().ForEach(hM => this.WriteOneWayMethod(hM, hSb));
            Service.Pair.Client.Rpcs.ToList().ForEach(hM => this.WriteSessionEvent(hM, hSb));
            return hSb.ToString();
        }
    }
}
