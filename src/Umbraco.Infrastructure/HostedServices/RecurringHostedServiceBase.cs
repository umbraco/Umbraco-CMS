using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Infrastructure.HostedServices
{
    /// <summary>
    /// Provides a base class for recurring background tasks implemented as hosted services.
    /// </summary>
    /// <remarks>
    /// See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio#timed-background-tasks
    /// </remarks>
    public abstract class RecurringHostedServiceBase : IHostedService, IDisposable
    {
        protected const int DefaultDelayMilliseconds = 180000; // 3 mins

        private readonly int _periodMilliseconds;
        private readonly int _delayMilliseconds;
        private Timer _timer;

        protected RecurringHostedServiceBase(int periodMilliseconds, int delayMilliseconds)
        {
            _periodMilliseconds = periodMilliseconds;
            _delayMilliseconds = delayMilliseconds;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteAsync, null, _delayMilliseconds, _periodMilliseconds);
            return Task.CompletedTask;
        }

        public abstract void ExecuteAsync(object state);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
