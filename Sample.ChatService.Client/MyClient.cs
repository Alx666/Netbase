using System;
using Sample.ChatService.Protocol;

namespace Sample.ChatService.Client
{
    internal class MyClient : ChatSession
    {
        public override void OnForwardMessage(string sSender, string sMessage)
        {
            Console.WriteLine(sSender + ": " + sMessage);
        }
    }
}
