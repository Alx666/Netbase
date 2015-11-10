using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Test.LongCalls.Client
{
    internal class LongCallsClient : Test.LongCalls.Protocol.LongCallsSession
    {
        public LongCallsClient()
        {
        }
        public override int OnTimedCallCB(int iSeconds)
        {
            Thread.Sleep(iSeconds * 100);
            return iSeconds;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("type calls duration in seconds whitespace separated");
            Console.WriteLine("Sample: 3 2 1 5");

            using (LongCallsClient hSession = new LongCallsClient())
            {
                hSession.Connect("127.0.0.1", 28000);

                while (true)
                {
                    Console.Write("> ");
                    List<int> hValues = Console.ReadLine().Trim().Split(new char[] { ' ' }).Select(hV => int.Parse(hV)).ToList();

                    hValues.AsParallel().ForAll(iTime =>
                    {
                        Console.WriteLine("Sending " + iTime);
                        int iReturn = hSession.TimedCall(iTime);
                        Console.WriteLine(iReturn + " Completed");
                    });               
                }
            }
        }
    }
}
