// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Helper class to not repeat common patterns with Task.
    /// </summary>
    public class TaskHelper
    {
        private readonly ILogger<TaskHelper> _logger;

        public TaskHelper(ILogger<TaskHelper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs a TPL Task fire-and-forget style, the right way - in the
        /// background, separate from the current thread, with no risk
        /// of it trying to rejoin the current thread.
        /// </summary>
        public void RunBackgroundTask(Func<Task> fn) => Task.Run(LoggingWrapper(fn)).ConfigureAwait(false);

        /// <summary>
        /// Runs a task fire-and-forget style and notifies the TPL that this
        /// will not need a Thread to resume on for a long time, or that there
        /// are multiple gaps in thread use that may be long.
        /// Use for example when talking to a slow webservice.
        /// </summary>
        public void RunLongRunningBackgroundTask(Func<Task> fn) =>
            Task.Factory.StartNew(LoggingWrapper(fn), TaskCreationOptions.LongRunning)
                .ConfigureAwait(false);

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
}
