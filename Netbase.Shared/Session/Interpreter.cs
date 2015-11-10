using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.Shared
{
    public class Interpreter
    {
        private Dictionary<byte, IPool> m_hPackets;
        private Dictionary<byte, IPool> m_hPacketActions;
        private Dictionary<Type, byte>  m_hTypeDictionary;
        private IPoolAllocator          m_hAllocator;

        internal Interpreter(IPoolAllocator hPoolAllocator) 
        {
            m_hAllocator        = hPoolAllocator;
            m_hPackets          = new Dictionary<byte, IPool>();
            m_hPacketActions    = new Dictionary<byte, IPool>();
            m_hTypeDictionary   = new Dictionary<Type, byte>();    
        }

        internal Interpreter() : this(new DefaultPoolAllocator())
        {
   
        }

        public void RegisterActions(Type[] hActions)
        {
            MethodInfo hRegisterActionMethod    = this.GetType().GetMethod("RegisterAction");

            for (int i = 0; i < hActions.Length; i++)
            {
                hRegisterActionMethod.MakeGenericMethod(hActions[i]).Invoke(this, null);
            }
        }

        public void RegisterRequests(Type[] hPackets)
        {
            MethodInfo hRegisterMethod = this.GetType().GetMethod("Register");

            for (int i = 0; i < hPackets.Length; i++)
            {
                hRegisterMethod.MakeGenericMethod(hPackets[i]).Invoke(this, null);
            }
        }

        public void Warmup(int iAllocations)
        {
            foreach (IPool hPool in m_hPackets.Values)
            {
                List<Packet> hPackets = new List<Packet>();

                for (int i = 0; i < iAllocations; i++)
                {
                    hPackets.Add(hPool.Get());
                }

                hPackets.ForEach(hP => hP.Recycle());
            }

            foreach (IPool hPool in m_hPacketActions.Values)
            {
                List<Packet> hPackets = new List<Packet>();

                for (int i = 0; i < iAllocations; i++)
                {
                    hPackets.Add(hPool.Get());
                }

                hPackets.ForEach(hP => hP.Recycle());
            }
        }
                
        public IAction Get(byte bId)
        {
            IPool hPool;
            if (m_hPacketActions.TryGetValue(bId, out hPool))
            {
                return hPool.Get() as IAction;
            }
            else
            {
                throw new KeyNotFoundException("Missing Packet Pool");
            }
        }
        
        public T Get<T>() where T : Packet
        {            
            IPool hPool;
            byte bId;
            if (m_hTypeDictionary.TryGetValue(typeof(T), out bId))
            {
                if(m_hPackets.TryGetValue(bId, out hPool))
                {
                    return hPool.Get() as T;
                }
            }
            
            throw new KeyNotFoundException();
        }

        public void Recycle(Packet hPacket)
        {
            hPacket.Recycle();
        }

        public void RegisterAction<T>() where T : Packet, IAction, new()
        {
            NetbasePacket hAttribute = typeof(T).GetCustomAttributes(typeof(NetbasePacket), true).First() as NetbasePacket;

            IPool hNewPool = m_hAllocator.Get<T>();

            m_hPacketActions.Add(hAttribute.Id, hNewPool);
            m_hTypeDictionary.Add(typeof(T), hAttribute.Id);
        }

        public void Register<T>() where T : Packet, new()
        {
            NetbasePacket hAttribute = typeof(T).GetCustomAttributes(typeof(NetbasePacket), true).First() as NetbasePacket;

            IPool hNewPool = m_hAllocator.Get<T>();

            m_hPackets.Add(hAttribute.Id, hNewPool);
            m_hTypeDictionary.Add(typeof(T), hAttribute.Id);
        }


        internal interface IPoolAllocator
        {
            IPool Get<T>() where T : Packet, new();
        }

        private class DefaultPoolAllocator : IPoolAllocator
        {
            public IPool Get<T>() where T : Packet, new()
            {
                return new PacketPool<T>();
            }
        }
    }
}
