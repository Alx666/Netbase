using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ServiceContract : Attribute, IServiceAttribute
    {
        public string   Name    { get; private set; }
        public Type     Service { get; private set; }

        public ServiceContract(string sServiceName, Type hService)
        {
            Name    = sServiceName;
            Service = hService;
        }
    }

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class CallbackContract : Attribute, IServiceAttribute
    {
        public string   Name    { get; private set; }
        public Type     Service { get; private set; }

        public CallbackContract(string sServiceName, Type hService)
        {
            Name    = sServiceName;
            Service = hService;
        }
    }

    public interface IServiceAttribute
    {
        string  Name    { get; }
        Type    Service { get; }
    }



}
