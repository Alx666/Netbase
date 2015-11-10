using Netbase.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Netbase.CodeGen;

namespace Netbase
{
    internal class RpcServicePair
    {
        public int ProtocolCounter              { get; set; }
        public RpcService   Server              { get; private set; }
        public RpcService   Client              { get; private set; }
        public List<string> SharedCode          { get; private set; }
        public List<string> ServiceCode         { get; private set; }

        public RpcServicePair(string sServerNamespace, string sClientNamespace, IServiceAttribute hServerAttrib, IServiceAttribute hClientAttrib)
        {
            Server      = new RpcService(sServerNamespace, this, hClientAttrib.Service, hServerAttrib, ".Service");
            Client      = new RpcService(sClientNamespace, this, hServerAttrib.Service, hClientAttrib, ".Protocol");
            Client.Generate();
            Server.Generate();

            SharedCode  = new List<string>();
            SharedCode.AddRange(Server.Rpcs.Select(hM => hM.Request.Code));
            SharedCode.AddRange(Server.Rpcs.Where(hM  => hM.Response != null).Select(hM => hM.Response.Code));
            SharedCode.AddRange(Server.Rpcs.Where(hM  => hM.ResponseAction != null).Select(hM => hM.ResponseAction.Code));        
   
            SharedCode.AddRange(Client.Rpcs.Select(hM => hM.Request.Code));
            SharedCode.AddRange(Client.Rpcs.Where(hM => hM.Response != null).Select(hM => hM.Response.Code));
            SharedCode.AddRange(Client.Rpcs.Select(hM => hM.RequestAction.Code));
            SharedCode.Add(new ClientSessionCodeGen(Client).Code);

            ServiceCode = new List<string>();
            ServiceCode.AddRange(Server.Rpcs.Select(hM => hM.RequestAction.Code));
            ServiceCode.AddRange(Client.Rpcs.Where(hM => hM.ResponseAction != null).Select(hM => hM.ResponseAction.Code));
            ServiceCode.Add(new ServerCodeGen(Server).Code);
            ServiceCode.Add(new ServerSessionCodeGen(Server).Code);
            
        }

    }


}
