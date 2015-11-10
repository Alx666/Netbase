using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class DataContract : System.Attribute
    {
    }
}
