// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;

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

        private readonly ILogger<RecurringHostedServiceBase> _logger;
        private TimeSpan _period;
        private readonly TimeSpan _delay;
        private Timer _timer;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringHostedServiceBase"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="period">Timespan representing how often the task should recur.</param>
        /// <param name="delay">Timespan representing the initial delay after application start-up before the first run of the task occurs.</param>
        protected RecurringHostedServiceBase(ILogger<RecurringHostedServiceBase> logger, TimeSpan period, TimeSpan delay)
        {
            _logger = logger;
            _period = period;
            _delay = delay;
        }

        // Scheduled for removal in V11
        [Obsolete("Please use constructor that takes an ILogger instead")]
        protected RecurringHostedServiceBase(TimeSpan period, TimeSpan delay)
        : this(StaticApplicationLogging.CreateLogger<RecurringHostedServiceBase>(), period, delay)
        {
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (!ExecutionContext.IsFlowSuppressed() ? (IDisposable)ExecutionContext.SuppressFlow() : null)
            {
                _timer = new Timer(ExecuteAsync, null, (int)_delay.TotalMilliseconds, (int)_period.TotalMilliseconds);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="state">The task state.</param>
        public async void ExecuteAsync(object state)
        {
            try
            {
                // First, stop the timer, we do not want tasks to execute in parallel
                _timer?.Change(Timeout.Infinite, 0);

                // Delegate work to method returning a task, that can be called and asserted in a unit test.
                // Without this there can be behaviour where tests pass, but an error within them causes the test
                // running process to crash.
                // Hat-tip: https://stackoverflow.com/a/14207615/489433
                await PerformExecuteAsync(state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in recurring hosted service {serviceName}.");
            }
            finally
            {
                // Resume now that the task is complete - Note we use period in both because we don't want to execute again after the delay.
                // So first execution is after _delay, and the we wait _period between each
                _timer?.Change((int)_period.TotalMilliseconds, (int)_period.TotalMilliseconds);
            }
        }

        public abstract Task PerformExecuteAsync(object state);

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _period = Timeout.InfiniteTimeSpan;
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                     _timer?.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
