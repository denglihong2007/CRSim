using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRSim.Core.Abstractions
{
    public interface IFlagService
    {
        Task WaitCloseAsync(CancellationToken token = default);
        void SetCloseFlag();
    }
}
