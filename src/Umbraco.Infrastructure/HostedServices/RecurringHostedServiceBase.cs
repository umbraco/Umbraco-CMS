// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Notifications;
using Umbraco.Cms.Web.Common.DependencyInjection;

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
        private readonly IEventAggregator _eventAggregator;

        public TimeSpan Period { get; }
        public TimeSpan Delay { get; }

        private Timer _timer;

        [Obsolete("Use the ctor that inject parameters")]
        protected RecurringHostedServiceBase(TimeSpan period, TimeSpan delay) : this(period, delay, StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringHostedServiceBase"/> class.
        /// </summary>
        /// <param name="period">Timepsan representing how often the task should recur.</param>
        /// <param name="delay">Timespan represeting the initial delay after application start-up before the first run of the task occurs.</param>
        protected RecurringHostedServiceBase(TimeSpan period, TimeSpan delay, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Period = period;
            Delay = delay;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var schedulingNotification = new RecurringHostedServiceSchedulingNotification(this, new EventMessages());
            _eventAggregator.PublishAsync(schedulingNotification);
            _timer = new Timer(ExecuteAsync, null, (int)Delay.TotalMilliseconds, (int)Period.TotalMilliseconds);

            _eventAggregator.PublishAsync(new RecurringHostedServiceScheduledNotification(this, new EventMessages()).WithStateFrom(schedulingNotification));
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
                var executingNotification = new RecurringHostedServiceExecutingNotification(this, new EventMessages());
                await _eventAggregator.PublishAsync(executingNotification);
                // First, stop the timer, we do not want tasks to execute in parallel
                _timer?.Change(Timeout.Infinite, 0);

                // Delegate work to method returning a task, that can be called and asserted in a unit test.
                // Without this there can be behaviour where tests pass, but an error within them causes the test
                // running process to crash.
                // Hat-tip: https://stackoverflow.com/a/14207615/489433
                await PerformExecuteAsync(state);
                await _eventAggregator.PublishAsync(new RecurringHostedServiceExecutedNotification(this, new EventMessages()).WithStateFrom(executingNotification));
            }
            catch (Exception ex)
            {
                await _eventAggregator.PublishAsync(new RecurringHostedServiceFailedNotification(this, new EventMessages()));
            }
            finally
            {
                var reschedulingNotification = new RecurringHostedServiceReschedulingNotification(this, new EventMessages());
                // Resume now that the task is complete - Note we use period in both because we don't want to execute again after the delay.
                // So first execution is after _delay, and the we wait _period between each
                await _eventAggregator.PublishAsync(reschedulingNotification);
                _timer?.Change((int)Period.TotalMilliseconds, (int)Period.TotalMilliseconds);
                await _eventAggregator.PublishAsync(new RecurringHostedServiceRescheduledNotification(this, new EventMessages()).WithStateFrom(reschedulingNotification));
            }
        }

        public abstract Task PerformExecuteAsync(object state);

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            var stoppingNotification = new RecurringHostedServiceStoppingNotification(this, new EventMessages());
            _eventAggregator.PublishAsync(stoppingNotification);
            _timer?.Change(Timeout.Infinite, 0);
            _eventAggregator.PublishAsync(new RecurringHostedServiceStoppedNotification(this, new EventMessages()).WithStateFrom(stoppingNotification));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() => _timer?.Dispose();
    }
}
