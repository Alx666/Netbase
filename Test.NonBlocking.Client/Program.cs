﻿using System;
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

        public Client()
        {
            m_hThread = new Thread(ThreadRoutine);
            m_hThread.Start();
        }

        public override string OnGetRandomString()
        {
            return Program.RandomStrings("abcdefghilmnopqrstuvz1234567890", 1, 50, 1, new Random()).First();
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


            //Test: Connect/Disconnect
            m_hClient.Connected     += OnConnect;
            m_hClient.Disconnected  += OnDisconnectTest;
            Console.Write("Testing Connect/Disconnect...");
            OnDisconnectTest();
            m_hEvent.WaitOne();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t\t\tPassed!");
            Console.ForegroundColor = ConsoleColor.Gray;
            m_hClient.Connected     -= OnConnect;
            m_hClient.Disconnected  -= OnDisconnectTest;
            m_hClient.Disconnected  += OnDisconnect;
            Console.WriteLine();


            //Test: Data Integrity
            Console.WriteLine("Testing Rpc Data Integrity...");
            Console.Write("Min String Size> ");
            int iMin = int.Parse(Console.ReadLine());
            Console.Write("Max String Size> ");
            int iMax = int.Parse(Console.ReadLine());
            Console.Write("String Count> ");
            int iCount = int.Parse(Console.ReadLine());


            Random hRand = new Random();
            for (int i = 0; i < iCount; i++)
            {
                string sText = RandomStrings("qwertyuiopasdfghjklzxcvbnm", iMin, iMax, 1, hRand).First();
                SentHash = sText.GetHashCode();
                Console.Write(SentHash + "\t\t=>\t\t");
                m_hClient.Echo(sText, OnTestDataIntegrity);
                m_hEvent.WaitOne();
            }


            //Test: CallbackService
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Testing Callback Calls...");
            m_hClient.BeginTestCallbacks(() => { m_hEvent.Set(); });
            m_hEvent.WaitOne();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t\t\tPassed!");
            Console.ForegroundColor = ConsoleColor.Gray;

            //Test: Big Data
            //TODO: need support for PacketSize

            //Test: Recurring Calls
            //TODO Recurring Calls for NonBlocking Session
            //Console.WriteLine("Testing Recurring Calls...");
            //Console.Write("Enter Recursions Count> ");
            //int iInput = int.Parse(Console.ReadLine());
            //m_hClient.RecurringServer(iInput, OnRecusionTest);
            //m_hEvent.WaitOne();

            //Test: Calls Ordering




            Console.WriteLine("Press Any Key To Exit");
            Console.ReadLine();
        }

        private static void OnDisconnectTest()
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

        private static void OnRecusionTest()
        {
            Console.WriteLine(" Passed!");
            m_hEvent.Set();
        }


        private static void OnDisconnect()
        {
            Console.WriteLine("Disconnected");
        }


        public static IEnumerable<string> RandomStrings(string allowedChars, int minLength, int maxLength, int count, Random rng)
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
