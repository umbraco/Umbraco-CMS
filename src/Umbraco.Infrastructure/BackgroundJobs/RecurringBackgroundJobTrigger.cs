// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Default implementation of <see cref="IRecurringBackgroundJobTrigger{TJob}" /> that delegates to the hosted service runner.
/// </summary>
/// <typeparam name="TJob">The type of the recurring background job to trigger.</typeparam>
internal sealed class RecurringBackgroundJobTrigger<TJob> : IRecurringBackgroundJobTrigger<TJob>
    where TJob : ITriggerableRecurringBackgroundJob
{
    private readonly RecurringBackgroundJobHostedServiceRunner _runner;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobTrigger{TJob}" /> class.
    /// </summary>
    /// <param name="runner">The runner.</param>
    public RecurringBackgroundJobTrigger(RecurringBackgroundJobHostedServiceRunner runner)
        => _runner = runner;

    /// <inheritdoc />
    public bool TriggerExecution()
        => TriggerExecution(NextExecutionStrategy.None);

    /// <inheritdoc />
    public bool TriggerExecution(NextExecutionStrategy strategy)
        => _runner.TriggerExecution<TJob>(strategy);

    /// <inheritdoc />
    public bool TriggerExecution(TimeSpan nextDelay)
        => _runner.TriggerExecution<TJob>(nextDelay);
}
