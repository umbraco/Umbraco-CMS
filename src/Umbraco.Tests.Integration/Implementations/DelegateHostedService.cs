using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Tests.Integration
{
    /// <summary>
    /// Executes arbitrary code on start/end
    /// </summary>
    public class DelegateHostedService : IHostedService
    {
        private readonly Action _start;
        private readonly Action _end;

        public static DelegateHostedService Create(Action start, Action end) => new DelegateHostedService(start, end);

        private DelegateHostedService(Action start, Action end)
        {
            _start = start;
            _end = end;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _end?.Invoke();
            return Task.CompletedTask;
        }
    }

   
}
