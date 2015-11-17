using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Test.NonBlocking.Protocol;

namespace Test.NonBlocking.Client
{
    internal class Client : NonBlockingTestCallback
    {
        private Thread m_hThread;
        private AutoResetEvent m_hEvent;
        private string m_sMessage;

        public Client()
        {
            m_hThread = new Thread(ThreadRoutine);
            m_hEvent = new AutoResetEvent(false);
            m_hThread.Start();
        }
        
        public override string OnRecurringClient(string sMessage, int iCount)
        {
            if (iCount == 0)
                return sMessage;
            else
            {
                this.RecurringServer(sMessage + iCount.ToString(), --iCount, OnCallEnd);
                m_hEvent.WaitOne();
                return m_sMessage;
            }
        }

        
        private void OnCallEnd(string obj)
        {
            m_sMessage = obj;
            m_hEvent.Set();
        }



        private void ThreadRoutine()
        {
            while (true)
            {
                this.Update();
            }
        }
    }

    class Program
    {
        private static int Connections = 10;
        private static Client m_hClient;
        private static AutoResetEvent m_hEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.Write("Number of Connects: ");
            Connections = int.Parse(Console.ReadLine());

            m_hClient = new Client();
            m_hClient.Disconnected  += OnDisconnect;


            //Test: Connect/Disconnect
            m_hClient.Connected     += OnConnect;
            Console.Write("Testing Connect/Disconnect...");
            OnDisconnect();
            m_hEvent.WaitOne();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Passed!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();


            //Test: Data Integrity
            Console.WriteLine("Testing Rpc Data Integrity...");
            Console.Write("Min String Size> ");
            int iMin = int.Parse(Console.ReadLine());
            Console.Write("Max String Size> ");
            int iMax = int.Parse(Console.ReadLine());
            Console.Write("String Count> ");
            int iCount = int.Parse(Console.ReadLine());

            m_hClient.Connected -= OnConnect;            
            Random hRand = new Random();
            for (int i = 0; i < iCount; i++)
            {
                string sText = RandomStrings("qwertyuiopasdfghjklzxcvbnm", iMin, iMax, 1, hRand).First();
                SentHash = sText.GetHashCode();
                Console.Write(SentHash + "\t\t=>\t\t");
                m_hClient.Echo(sText, OnTestDataIntegrity);
                m_hEvent.WaitOne();
            }

            //Test: Big Data
            //TODO: need support for PacketSize

            //Test: Recurring Calls
            //TODO Recurring Calls for NonBlocking Session
            //Console.WriteLine("Testing Recurring Calls...");
            //Console.Write("Enter Text> ");
            //string sInput = Console.ReadLine();
            //m_hClient.RecurringServer(sInput, 10, OnRecurringText);
            //m_hEvent.WaitOne();

            //Test: Calls Ordering




            Console.WriteLine("Press Any Key To Exit");
            Console.ReadLine();
        }

        private static void OnDisconnect()
        {
            if(Connections > 0)
                m_hClient.Connect("127.0.0.1", 28000);
        }

        private static int SentHash;
        private static int RecvHash;
        private static void OnTestDataIntegrity(string sCb)
        {
            RecvHash = sCb.GetHashCode();
            Console.Write(RecvHash + "...");
            if (SentHash == RecvHash)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tMatch " + Encoding.Unicode.GetBytes(sCb).Length + " bytes");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\tMismatch" + Encoding.Unicode.GetBytes(sCb).Length + " bytes");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            m_hEvent.Set();
        }

        private static void OnConnect()
        {
            Connections--;
            Connections.ToString().ToList().ForEach(x => Console.Write(" "));            
            Console.CursorLeft = Console.CursorLeft - Connections.ToString().Length;
            Console.Write(Connections);
            Console.CursorLeft = Console.CursorLeft - Connections.ToString().Length;

            if (Connections > 0)
                m_hClient.Close();
            else
                m_hEvent.Set();
        }

        private static void OnRecurringText(string sRet)
        {
            Console.WriteLine(sRet + " Passed!");
            m_hEvent.Set();
        }





        private static IEnumerable<string> RandomStrings(string allowedChars, int minLength, int maxLength, int count, Random rng)
        {
            char[] chars = new char[maxLength];
            int setLength = allowedChars.Length;

            while (count-- > 0)
            {
                int length = rng.Next(minLength, maxLength + 1);

                for (int i = 0; i < length; ++i)
                {
                    chars[i] = allowedChars[rng.Next(setLength)];
                }

                yield return new string(chars, 0, length);
            }
        }
    }
}
