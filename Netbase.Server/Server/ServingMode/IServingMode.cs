using Netbase.Shared;

namespace Netbase.Server
{
    internal interface IServingMode
    {
        void Execute(IAction hAction);
    }
}