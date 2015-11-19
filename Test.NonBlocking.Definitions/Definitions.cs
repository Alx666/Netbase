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
        void BeginTestCallbacks();

        [ServiceOperation]
        void RecurringServer(int iCount);
    }

    [CallbackContract("TestCallback", typeof(ITestService))]
    public interface ITestCallback
    {
        [ServiceOperation]
        void RecurringClient(int iCount);

        [ServiceOperation]
        string GetRandomString();
    }
}
