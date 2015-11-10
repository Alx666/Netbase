using Netbase.Shared;
namespace Netbase.Server
{
    /// <summary>
    /// Queued Execution of Service Method
    /// </summary>
    internal class ServingModeQueue : IServingMode
    {
        private IServerIOCP m_hService;
        private ISession    m_hContext;

        public ServingModeQueue(IServerIOCP hService, ISession hContext)
        {
            m_hService = hService;
            m_hContext = hContext;
        }

        public void Execute(IAction hAction)
        {
            hAction.Execute(m_hService, m_hContext);
        }
    }
}
