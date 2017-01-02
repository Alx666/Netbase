using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Netbase.Shared.Collections
{
    public class ConcurrentQueue<T> : ConcurrentContainer
    {
        private Queue<T>    m_hQueue;

        public ConcurrentQueue() : base()
        {
            m_hQueue        = new Queue<T>();
        }

        public void Enqueue(T item)
        {
            this.SpinWaitFor(() => { m_hQueue.Enqueue(item); });
        }

        public bool TryDequeue(out T result)
        {
            T extracted = default(T);
            bool bDone  = false;

            this.SpinWaitFor(() => 
            {
                if (m_hQueue.Count > 0)
                {
                    extracted   = m_hQueue.Dequeue();
                    bDone       = true;
                }
            });

            result = extracted;
            return bDone;
        }

        public bool TryPeek(out T result)
        {
            T extracted = default(T);
            bool bDone  = false;

            this.SpinWaitFor(() =>
            {
                if (m_hQueue.Count > 0)
                {
                    extracted = m_hQueue.Peek();
                    bDone = true;
                }
            });

            result = extracted;
            return bDone;
        }

        public int Count
        {
            get
            {
                int iCount = 0;

                this.SpinWaitFor(() => 
                {
                    iCount = m_hQueue.Count;
                });

                return iCount;
            }
        }
    }
}
