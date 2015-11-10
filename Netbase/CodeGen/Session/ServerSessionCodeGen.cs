using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class ServerSessionCodeGen : SessionCodeGen
    {
        public ServerSessionCodeGen(RpcService hService) : base(hService)
        {

        }

        protected override string FirstUsingDirective()
        {
            return "using Netbase.Server;";
        }

        protected override string SecondUsingDirective()
        {
            return string.Format("using {0};", Service.Pair.Client.Namespace);
        }

        protected override string BaseType()
        {
            return "SessionIOCP";
        }

        protected override string PacketRegistration()
        {
            return string.Empty;
        }

        protected override string Name()
        {
            return string.Format("{0}Context", Service.Name);
        }

        protected override string WriteRpcMethods()
        {
            StringBuilder hSb = new StringBuilder();
            Service.Pair.Client.Rpcs.Where(hR => hR.ResponseAction != null).ToList().ForEach(hM => this.WriteMethod(hM, hSb));
            Service.Pair.Client.Rpcs.Where(hR => hR.Response == null).ToList().ForEach(hM => this.WriteOneWayMethod(hM, hSb));
            return hSb.ToString();
        }
    }
}
