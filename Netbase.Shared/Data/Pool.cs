using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    public class Pool<T> where 
        T : class, new()
    {

        private Queue<T> m_hElements;
        public Pool()
        {
            m_hElements = new Queue<T>();
        }

        public T Get()
        {
            lock (m_hElements)
            {
                if (m_hElements.Count > 0)
                {
                    return m_hElements.Dequeue();
                }
                else
                {
                    return new T();
                }
            }
        }

        public void Recycle(T hElem)
        {
            lock (m_hElements)
            {
                m_hElements.Enqueue(hElem);
            }
        }
    }
}
