using CRSim.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRSim.Core.Services
{
    public sealed class FlagService : IFlagService
    {
        private readonly TaskCompletionSource<bool> _tcs = new();
        
        public FlagService()
        {
            Console.WriteLine($"SetCloseFlag {GetHashCode()}");
        }

        public Task WaitCloseAsync(CancellationToken token = default)
            => _tcs.Task;

        public void SetCloseFlag()
        {
            Console.WriteLine("#34");
            _tcs.TrySetResult(true);
        }
    }

}
