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
        protected static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);

        private readonly TimeSpan _period;
        private readonly TimeSpan _delay;
        private Timer _timer;

        protected RecurringHostedServiceBase(TimeSpan period, TimeSpan delay)
        {
            _period = period;
            _delay = delay;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteAsync, null, (int)_delay.TotalMilliseconds, (int)_period.TotalMilliseconds);
            return Task.CompletedTask;
        }

        public async void ExecuteAsync(object state)
        {
            // Delegate work to method returning a task, that can be called and asserted in a unit test.
            // Without this there can be behaviour where tests pass, but an error within them causes the test
            // running process to crash.
            // Hat-tip: https://stackoverflow.com/a/14207615/489433
            await PerformExecuteAsync(state);
        }

        internal abstract Task PerformExecuteAsync(object state);

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
