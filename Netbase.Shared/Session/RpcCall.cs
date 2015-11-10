using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Netbase.Shared
{
    public class RpcCall : IDisposable
    {
        public AutoResetEvent   WaitEvent   { get; private set; }
        public object           Data        { get; set; }
        public bool             Async       { get; set; }
        public object           Cb          { get; set; }

        public RpcCall()
        {
            WaitEvent = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            WaitEvent.Close();
        }

        public void Wait()
        {
            WaitEvent.WaitOne();
        }

        public void Set()
        {
            WaitEvent.Set();
        }
    }
}
