// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Provides methods to signal a specific recurring background job to execute immediately.
/// </summary>
/// <typeparam name="TJob">The type of the recurring background job to trigger. Must implement <see cref="ITriggerableRecurringBackgroundJob" />.</typeparam>
public interface IRecurringBackgroundJobTrigger<TJob>
    where TJob : ITriggerableRecurringBackgroundJob
{
    /// <summary>
    /// Signals the background loop to execute immediately.
    /// After the triggered execution, the original schedule is kept.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the job was found and triggered; <c>false</c> if no hosted service is running for this job type.
    /// </returns>
    /// <seealso cref="NextExecutionStrategy.None" />
    bool TriggerExecution();

    /// <summary>
    /// Signals the background loop to execute immediately, with the specified strategy for determining the next execution after the triggered one completes.
    /// </summary>
    /// <param name="strategy">Controls the delay after the triggered execution.</param>
    /// <returns>
    ///   <c>true</c> if the job was found and triggered; <c>false</c> if no hosted service is running for this job type.
    /// </returns>
    bool TriggerExecution(NextExecutionStrategy strategy);

    /// <summary>
    /// Signals the background loop to execute immediately.
    /// After the triggered execution, the next execution is scheduled after the specified delay (measured from execution start; execution time is subtracted to prevent drift).
    /// </summary>
    /// <param name="nextDelay">The target interval from execution start to the next execution. Execution time is subtracted to prevent drift.</param>
    /// <returns>
    ///   <c>true</c> if the job was found and triggered; <c>false</c> if no hosted service is running for this job type.
    /// </returns>
    bool TriggerExecution(TimeSpan nextDelay);
}
