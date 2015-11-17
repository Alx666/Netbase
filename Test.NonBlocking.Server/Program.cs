using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Test.NonBlocking.Service;

namespace Test.NonBlocking.Server
{
    internal class Context : TestServiceContext
    {
    }

    internal class Server : TestService<Context>
    {
        public override string Echo(Context hContext, string sMessage)
        {
            Console.WriteLine(sMessage.GetHashCode());
            return sMessage;
        }

        public override string RecurringServer(Context hContext, string sMessage)
        {
            return null;
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            using (Server hServ = new Server())
            {
                hServ.Start(IPAddress.Any, 28000);
                hServ.WaitForExit();
            }
        }
    }
}
