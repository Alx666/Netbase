using System;
using System.Net;
using Test.NestedCalls.Service;

namespace Test.NestedCalls.Server
{
    internal class NestedCallsContex : NestedCallsServiceContext
    {
    }

    internal class NestedCallsServer : NestedCallsService<NestedCallsContex>
    {
        public override int SomeMethod(NestedCallsContex hContex, int i)
        {
            Console.WriteLine(i);

            if (i > 0)
            {
                return hContex.SomeCBMethod(i - 1);
            }
            else
            {
                return i;
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            using (NestedCallsServer hServer = new NestedCallsServer())
            {
                hServer.Start(IPAddress.Any, 28000);
                hServer.WaitForExit();
            }
        }
    }
}
