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
    internal class RpcMethodInfo
    {
        public RpcService               Service         { get; private set; }
        public MethodInfo               Method          { get; private set; }
        public ServiceOperation         Attribute       { get; private set; }
        public RequestCodeGen           Request         { get; private set; }
        public ResponseCodeGen          Response        { get; private set; }
        public RequestActionCodeGen     RequestAction   { get; private set; }
        public ResponseActionCodeGen    ResponseAction  { get; private set; }

        public RpcMethodInfo(RpcService hService, MethodInfo hMethod)
        {
            Method      = hMethod;
            Attribute   = hMethod.GetCustomAttribute<ServiceOperation>();
            Service     = hService;

            if (Attribute.Type == RpcType.OneWay && hMethod.ReturnType != typeof(void))
                throw new Exception("OneWay Rpc's require void return type, check method signature " + hMethod.Name);

            Request     = new RequestCodeGen(this, Service.Pair.ProtocolCounter);
            Service.Pair.ProtocolCounter++;

            if (Attribute.Type == RpcType.TwoWay)
            {
                Response = new ResponseCodeGen(this, Service.Pair.ProtocolCounter);
                Service.Pair.ProtocolCounter++;
            }

            if (hService.Attribute is ServiceContract && Attribute.Type == RpcType.OneWay)
            {
                RequestAction = new RequestActionCodeGen(new OneWayServerReqActionBuilder(), this);
            }
            else if (hService.Attribute is ServiceContract)
            {
                ResponseAction = new ResponseActionCodeGen(new ServerResActionBuilder(), this);
                RequestAction  = new RequestActionCodeGen(new TwoWayServerReqActionBuilder(), this);
            }
            else if (hService.Attribute is CallbackContract && Attribute.Type == RpcType.OneWay)
            {
                RequestAction = new RequestActionCodeGen(new OneWayClientReqActionBuilder(), this);
            }
            else
            {
                ResponseAction = new ResponseActionCodeGen(new ClientResActionBuilder(), this);
                RequestAction = new RequestActionCodeGen(new TwoWayClientReqActionBuilder(), this);
            }
        }


    }
}
