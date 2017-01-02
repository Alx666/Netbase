using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Netbase.Shared.Collections
{

    public class ConcurrentQueue<T>
    {
        private Queue<T> m_hQueue;
        private int m_iTokenCounter = int.MaxValue;
        private int m_iServedToken;

        public ConcurrentQueue()
        {
            m_hQueue        = new Queue<T>();
            m_iServedToken  = m_iTokenCounter + 1;
        }

        private void SpinWaitFor(Action hAction)
        {
            //Get an access token
            int iThreadToken = Interlocked.Increment(ref m_iTokenCounter);

            //Let the thread spin until 
            while (true)
            {                
                if (Interlocked.CompareExchange(ref iThreadToken, m_iServedToken + 1, m_iServedToken) != iThreadToken)
                {
                    hAction.Invoke();
                    m_iServedToken = iThreadToken;
                    break;
                }
            }            
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
