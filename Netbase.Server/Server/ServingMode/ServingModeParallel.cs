using Netbase.Shared;
using System;
using System.Threading.Tasks;

namespace Netbase.Server
{
    /// <summary>
    /// Parallel Execution of Service Method
    /// </summary>
    internal class ServingModeParallel : IServingMode
    {
        private IServerIOCP m_hService;
        private ISession    m_hContext;

        public ServingModeParallel(IServerIOCP hService, ISession hContext)
        {
            m_hService = hService;
            m_hContext = hContext;
        }

        public void Execute(IAction hAction)
        {
            Task.Factory.StartNew(() => Execution(hAction));
        }

        private void Execution(IAction hAction)
        {
            try
            {
                hAction.Execute(m_hService, m_hContext);
            }
            catch (Exception hEx)
            {
                //Todo: error handling, creare un handler con una coda concorrente e accodare xche possono arrivare sia da piu thread contemporaneamente
                Console.WriteLine("Error Executing Action " + hEx);
            }
            finally
            {
                (hAction as Packet).Recycle();
            }

        }
    }
}
