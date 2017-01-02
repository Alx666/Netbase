using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Netbase.Shared.Collections
{
    public abstract class ConcurrentContainer
    {
        private int m_iTokenCounter;
        private int m_iServedToken;

        public ConcurrentContainer()
        {
            m_iServedToken = m_iTokenCounter + 1;
        }

        protected void SpinWaitFor(Action hAction)
        {
            //Get an access token
            int iThreadToken = Interlocked.Increment(ref m_iTokenCounter);

            //Let the thread spin until it's is own turn
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
    }
}
