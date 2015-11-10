using Netbase.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.Server
{
    public class InterpreterInfo
    {
        private ConcurrentDictionary<byte, IPool> m_hPackets;
        private ConcurrentDictionary<byte, IPool> m_hPacketActions;
        private ConcurrentDictionary<Type, byte>        m_hTypeDictionary;

        public InterpreterInfo(System.Collections.Concurrent.ConcurrentDictionary<byte, Shared.IPool> m_hPackets, System.Collections.Concurrent.ConcurrentDictionary<byte, Shared.IPool> m_hPacketActions, System.Collections.Concurrent.ConcurrentDictionary<Type, byte> m_hTypeDictionary)
        {
            // TODO: Complete member initialization
            this.m_hPackets = m_hPackets;
            this.m_hPacketActions = m_hPacketActions;
            this.m_hTypeDictionary = m_hTypeDictionary;
        }

        public override string ToString()
        {
            List<KeyValuePair<Type, byte>> hRegisteredTypes = m_hTypeDictionary.OrderBy(hT => hT.Value).ToList();

            StringBuilder hSb = new StringBuilder();
            hSb.AppendFormat("{0} Registered Packets{1}", hRegisteredTypes.Count, Environment.NewLine);

            foreach (KeyValuePair<Type, byte> hPair in hRegisteredTypes)
            {
                hSb.AppendFormat("\t[{0}]\t{1}{2}", hPair.Value, hPair.Key.Name, Environment.NewLine);
            }

            return hSb.ToString();
        }
    }
}
