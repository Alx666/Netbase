using Netbase.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netbase
{
    internal class RpcService
    {
        public string               Name        { get; private set; }
        public RpcServicePair       Pair        { get; private set; }
        public RpcMethodInfo[]      Rpcs        { get; private set; }
        public IServiceAttribute    Attribute   { get; private set; }
        public Type                 Interface   { get; private set; }
        public string               Namespace   { get; private set; }

        public RpcService(string sNamespace, RpcServicePair hPair, Type hInterface, IServiceAttribute hAttribute, string sNamespaceSuffix)
        {
            Name        = hAttribute.Name;
            Pair        = hPair;
            Interface   = hInterface;
            Attribute   = hAttribute;
            Namespace   = sNamespace;  
        }

        public void Generate()
        {
            List<RpcMethodInfo> hTmp = new List<RpcMethodInfo>();
            foreach (MethodInfo hMethod in Interface.GetMethods())
            {
                hTmp.Add(new RpcMethodInfo(this, hMethod));
            }

            Rpcs = hTmp.ToArray();
        }
    }
}
