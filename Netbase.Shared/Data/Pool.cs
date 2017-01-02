using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netbase.Shared.Collections;

namespace Netbase.Shared
{
    public class Pool<T> where T : class, new()
    {
        private ConcurrentQueue<T> m_hElements;
        public Pool()
        {
            m_hElements = new ConcurrentQueue<T>();
        }

        public T Get()
        {
            T hRes;
            if (m_hElements.TryDequeue(out hRes))
                return hRes;
            else
                return new T();
        }

        public void Recycle(T hElem)
        {
            m_hElements.Enqueue(hElem);
        }
    }
}
