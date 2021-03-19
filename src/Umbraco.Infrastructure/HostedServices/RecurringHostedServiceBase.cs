// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Infrastructure.HostedServices
{
    /// <summary>
    /// Provides a base class for recurring background tasks implemented as hosted services.
    /// </summary>
    /// <remarks>
    /// See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio#timed-background-tasks
    /// </remarks>
    public abstract class RecurringHostedServiceBase : IHostedService, IDisposable
    {
        /// <summary>
        /// The default delay to use for recurring tasks for the first run after application start-up if no alternative is configured.
        /// </summary>
        protected static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);

        private readonly TimeSpan _period;
        private readonly TimeSpan _delay;
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringHostedServiceBase"/> class.
        /// </summary>
        /// <param name="period">Timepsan representing how often the task should recur.</param>
        /// <param name="delay">Timespan represeting the initial delay after application start-up before the first run of the task occurs.</param>
        protected RecurringHostedServiceBase(TimeSpan period, TimeSpan delay)
        {
            _period = period;
            _delay = delay;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteAsync, null, (int)_delay.TotalMilliseconds, (int)_period.TotalMilliseconds);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="state">The task state.</param>
        public async void ExecuteAsync(object state) =>
            // Delegate work to method returning a task, that can be called and asserted in a unit test.
            // Without this there can be behaviour where tests pass, but an error within them causes the test
            // running process to crash.
            // Hat-tip: https://stackoverflow.com/a/14207615/489433
            await PerformExecuteAsync(state);

        public abstract Task PerformExecuteAsync(object state);

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() => _timer?.Dispose();
    }
}
