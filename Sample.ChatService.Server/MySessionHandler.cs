using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample.ChatService.Service;

namespace Sample.ChatService.Service
{
    internal class MySessionHandler : ChatServiceContext
    {
        public string   Username { get; set; }
        public bool     LoggedIn { get; set; }

        public MySessionHandler()
        {
            Username = "Unknow User";
        }
    }
}
