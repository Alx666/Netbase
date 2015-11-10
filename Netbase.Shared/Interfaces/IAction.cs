using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Netbase.Shared
{
    public interface IAction
    {
        void LoadData(BinaryReader hReader);
        
        void Execute(ISession hContext);
    }
}
