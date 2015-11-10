using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.Shared
{
    //public static class TypeLoader
    //{
    //    private static Type[] m_hProtocolTypes;

    //    static TypeLoader()
    //    {
    //        List<Assembly> hLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
    //        List<string> hLoadedPaths = hLoadedAssemblies.Select(a => a.Location).ToList();
    //        List<string> hReferencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToList();
    //        List<string> hToLoad = hReferencedPaths.Where(hR => !hLoadedPaths.Contains(hR, StringComparer.InvariantCultureIgnoreCase)).ToList();
    //        hToLoad.ForEach(hPath => hLoadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(hPath))));

    //        m_hProtocolTypes = (from hAssembly in hLoadedAssemblies
    //                                   from hType in hAssembly.GetTypes()
    //                                   from hAttrib in hType.GetCustomAttributes(typeof(NetbasePacket), true)
    //                                   where hAttrib != null
    //                                   select hType).ToArray();
    //    }

    //    public static Type[] Load<C>(string sServiceName) where C : class, INetbaseServiceReference
    //    {            
    //        return (from hT in m_hProtocolTypes
    //                from hA in hT.GetCustomAttributes(typeof(C), false)
    //                where (hA as C) != null && (hA as INetbaseServiceReference).ServiceName == sServiceName
    //                select hT).ToArray();
    //    }
    //}
}
