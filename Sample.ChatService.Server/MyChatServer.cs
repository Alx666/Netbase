using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample.ChatService.Service;
using Sample.ChatService.Protocol;

namespace Sample.ChatService.Server
{
    internal class MyChatServer : ChatService<MySessionHandler>
    {
        public override int Login(MySessionHandler hContex, string sUsername, string sPassword)
        {            
            hContex.Username = sUsername;
            hContex.LoggedIn = true;
            return 1;            
        }

        public override void Logout(MySessionHandler hContex)
        {
            hContex.Dispose();
        }

        public override void Message(MySessionHandler hContex, string sMessage)
        {           
            if (!hContex.LoggedIn)
                throw new Exception("Login First");

            Console.WriteLine(hContex.Username + ": " + sMessage);

            m_hClients.Values.Where(hC => hC != hContex).AsParallel().ForAll(hC => hC.ForwardMessage(hContex.Username, sMessage));
        }

        public override Vector3 GetVector(MySessionHandler hContext, VeryComplexType hComplex)
        {

            

            Vector3 vVector = new Vector3();
            vVector.x = hComplex.hData.somefloat;
            vVector.y = hComplex.hData.somefloat;
            vVector.z = hComplex.hData.somefloat;
            return vVector;
        }
    }
}
