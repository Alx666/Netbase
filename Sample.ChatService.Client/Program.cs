using System;
using Sample.ChatService.Protocol;

namespace Sample.ChatService.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using(MyNonBlockingClient hClient = new MyNonBlockingClient())
            {
                hClient.Connect("127.0.0.1", 6666);    
                
                while (true)
                {
                    Console.Write("> ");
                    string   sInput   = Console.ReadLine();
                    string[] hCommand = sInput.Split(new char[] { ' ' });
                    
                    if (hCommand.Length == 3 && hCommand[0] == "login")
                    {                        
                        //hClient.Login(hCommand[1], hCommand[2]);
                        hClient.Login(hCommand[1], hCommand[2], (i) => Console.WriteLine("Login Completed: " + i));                        
                    }
                    else if (hCommand.Length == 1 && hCommand[0] == "logout")
                    {
                        hClient.Logout(() => Console.WriteLine("Logged Out"));
                    }
                    else if (hCommand[0] == "GetVector")
                    {
                        //VeryComplexType hComplex = new VeryComplexType();
                        //hComplex.hData = new ComplexType();
                        //hComplex.hData.somefloat = 777f;
                        

                        //hClient.GetVector(hComplex);
                        //Console.WriteLine(vResult.x);
                    }
                    else
                    {
                        hClient.Message(sInput);
                    }
                }
            }
        }
    }
}
