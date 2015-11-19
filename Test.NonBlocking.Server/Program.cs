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
        public override void BeginTestCallbacks(Context hContext)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(hContext.GetRandomString());
            }

            hContext.GetRandomString((s) => { Console.WriteLine(s); });
        }

        public override string Echo(Context hContext, string sMessage)
        {
            Console.WriteLine(sMessage.GetHashCode());
            return sMessage;
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
