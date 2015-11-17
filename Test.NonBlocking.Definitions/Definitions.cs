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
        [ServiceOperation]
        string Echo(string sMessage);

        [ServiceOperation]
        string RecurringServer(string sMessage);
    }

    [CallbackContract("TestCallback", typeof(ITestService))]
    public interface ITestCallback
    {
        [ServiceOperation]
        string RecurringClient(string sMessage);
    }
}
