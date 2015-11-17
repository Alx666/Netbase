using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netbase.Shared;

namespace Test.NonBlocking.Definitions
{
    [ServiceContract("TestService", typeof(ITestCallback))]
    public interface ITestService
    {
        string TestMessage(string sMessage);
    }

    [CallbackContract("TestCallback", typeof(ITestService))]
    public interface ITestCallback
    {
        string ForwardTestMessage(string sMessage);
    }
}
