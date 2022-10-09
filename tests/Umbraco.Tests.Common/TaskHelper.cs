// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Tests.Common;

/// <summary>
///     Helper class to not repeat common patterns with Task.
/// </summary>
public sealed class TaskHelper
{
    private readonly ILogger<TaskHelper> _logger;

    public TaskHelper(ILogger<TaskHelper> logger) => _logger = logger;

    /// <summary>
    ///     Executes a fire and forget task outside of the current execution flow.
    /// </summary>
    public void RunBackgroundTask(Func<Task> fn) => ExecuteBackgroundTask(fn);

    // for tests, returning the Task as a public API indicates it can be awaited that is not what we want to do
    public Task ExecuteBackgroundTask(Func<Task> fn)
    {
        // it is also possible to use UnsafeQueueUserWorkItem which does not flow the execution context,
        // however that seems more difficult to use for async operations.

        // Do not flow AsyncLocal to the child thread
        using (ExecutionContext.SuppressFlow())
        {
            // NOTE: ConfigureAwait(false) is irrelevant here, it is not needed because this is not being
            // awaited. ConfigureAwait(false) is only relevant when awaiting to prevent the SynchronizationContext
            // (very different from the ExecutionContext!) from running the continuation on the calling thread.
            return Task.Run(LoggingWrapper(fn));
        }
    }

    /// <summary>
    ///     Executes a fire and forget task outside of the current execution flow on a dedicated (non thread-pool) thread.
    /// </summary>
    public void RunLongRunningBackgroundTask(Func<Task> fn) => ExecuteLongRunningBackgroundTask(fn);

    // for tests, returning the Task as a public API indicates it can be awaited that is not what we want to do
    public Task ExecuteLongRunningBackgroundTask(Func<Task> fn)
    {
        // it is also possible to use UnsafeQueueUserWorkItem which does not flow the execution context,
        // however that seems more difficult to use for async operations.

        // Do not flow AsyncLocal to the child thread
        using (ExecutionContext.SuppressFlow())
        {
            // NOTE: ConfigureAwait(false) is irrelevant here, it is not needed because this is not being
            // awaited. ConfigureAwait(false) is only relevant when awaiting to prevent the SynchronizationContext
            // (very different from the ExecutionContext!) from running the continuation on the calling thread.
            return Task.Factory.StartNew(LoggingWrapper(fn), TaskCreationOptions.LongRunning);
        }
    }

    // ensure any exceptions are handled and do not take down the app pool
    private Func<Task> LoggingWrapper(Func<Task> fn) =>
        async () =>
        {
            try
            {
                await fn();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown in a background thread");
            }
        };
}
