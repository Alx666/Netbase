using Sample.HelloWorld.Protocol;
using System;

namespace Sample.HelloWorld.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (HelloWorldSession hSession = new HelloWorldSession())
            {
                hSession.Connect("127.0.0.1", 28000);

                Console.WriteLine(hSession.GetMessageFromServer());
            }


            Console.ReadLine();
        }
    }
}
