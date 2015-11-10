using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ServiceOperation : Attribute
    {
        public RpcType Type { get; private set; }

        public ServiceOperation(RpcType eType = RpcType.TwoWay)
        {
            Type = eType;
        }
    }
}
