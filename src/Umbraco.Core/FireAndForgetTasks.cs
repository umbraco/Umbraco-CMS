// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core
{
    /// <summary>
    /// Helper class to deal with Fire and Forget tasks correctly.
    /// </summary>
    public class FireAndForgetTasks
    {
        private readonly ILogger<FireAndForgetTasks> _logger;

        public FireAndForgetTasks(ILogger<FireAndForgetTasks> logger) => _logger = logger;

        /// <summary>
        /// Runs a TPL Task fire-and-forget style, the right way - in the
        /// background, separate from the current thread, with no risk
        /// of it trying to rejoin the current thread.
        /// </summary>
        public Task RunBackgroundTask(Func<Task> fn)
        {
            using (ExecutionContext.SuppressFlow()) // Do not flow AsyncLocal to the child thread
            {
                Task t = Task.Run(LoggingWrapper(fn));
                t.ConfigureAwait(false);
                return t;
            }
        }

        /// <summary>
        /// Runs a task fire-and-forget style and notifies the TPL that this
        /// will not need a Thread to resume on for a long time, or that there
        /// are multiple gaps in thread use that may be long.
        /// Use for example when talking to a slow webservice.
        /// </summary>
        public Task RunLongRunningBackgroundTask(Func<Task> fn)
        {
            using (ExecutionContext.SuppressFlow())  // Do not flow AsyncLocal to the child thread
            {
                Task t = Task.Factory.StartNew(LoggingWrapper(fn), TaskCreationOptions.LongRunning);
                t.ConfigureAwait(false);
                return t;
            }
        }

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
