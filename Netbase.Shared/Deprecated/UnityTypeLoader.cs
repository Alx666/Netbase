using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.Shared
{
    //internal class UnityTypeLoader
    //{
    //    private Type[] m_hProtocolTypes;

    //    public UnityTypeLoader(Assembly hProtocolAssembly)
    //    {
    //        m_hProtocolTypes = (from hType in hProtocolAssembly.GetTypes()
    //                            from hAttrib in hType.GetCustomAttributes(typeof(NetbasePacket), true)
    //                            where hAttrib != null
    //                            select hType).ToArray();
    //    }

    //    public Type[] Load<C>(string sServiceName) where C : class, INetbaseServiceReference
    //    {
    //        return (from hT in m_hProtocolTypes
    //                from hA in hT.GetCustomAttributes(typeof(C), false)
    //                where (hA as C) != null && (hA as INetbaseServiceReference).ServiceName == sServiceName
    //                select hT).ToArray();
    //    }
    //}
}
