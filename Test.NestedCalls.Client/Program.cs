using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.NestedCalls.Protocol;

namespace Test.NestedCalls.Client
{
    internal class TestSession : NestedCallsSession
    {
        public override int OnSomeCBMethod(int i)
        {
            Console.WriteLine(i);

            if (i > 0)
                return this.SomeMethod(i - 1);
            else
                return 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (TestSession hSession = new TestSession())
            {
                hSession.Connect("127.0.0.1", 28000);

                while (true)
                {
                    Console.Write("Enter Iterations Number: ");

                    int iIterations = int.Parse(Console.ReadLine());

                    hSession.SomeMethod(iIterations);

                    Console.WriteLine("Done!");                    
                }
            }
        }
    }
}
