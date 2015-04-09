using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClientDependency.Core.Logging;
using Umbraco.Core.Logging;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Custom awaiter used to await when the BackgroundTaskRunner is completed (IsRunning == false)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This custom awaiter simply uses a TaskCompletionSource to set the result when the Completed event of the 
    /// BackgroundTaskRunner executes.
    /// A custom awaiter requires implementing INotifyCompletion as well as IsCompleted, OnCompleted and GetResult
    /// see: http://blogs.msdn.com/b/pfxteam/archive/2011/01/13/10115642.aspx
    /// </remarks>
    internal class BackgroundTaskRunnerAwaiter<T> : INotifyCompletion where T : class, IBackgroundTask
    {
        private readonly BackgroundTaskRunner<T> _runner;
        private readonly ILogger _logger;
        private readonly TaskCompletionSource<int> _tcs;
        private readonly TaskAwaiter<int> _awaiter;

        public BackgroundTaskRunnerAwaiter(BackgroundTaskRunner<T> runner, ILogger logger)
        {            
            if (runner == null) throw new ArgumentNullException("runner");
            if (logger == null) throw new ArgumentNullException("logger");
            _runner = runner;
            _logger = logger;

            _tcs = new TaskCompletionSource<int>();

            _awaiter = _tcs.Task.GetAwaiter();

            if (_runner.IsRunning)
            {
                _runner.Completed += (s, e) =>
                {
                    _logger.Debug<BackgroundTaskRunnerAwaiter<T>>("Setting result");

                    _tcs.SetResult(0);
                };                    
            }
            else
            {
                //not running, just set the result
                _tcs.SetResult(0);
            }
            
        }

        public BackgroundTaskRunnerAwaiter<T> GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// This is completed when the runner is finished running
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                _logger.Debug<BackgroundTaskRunnerAwaiter<T>>("IsCompleted :: " + _tcs.Task.IsCompleted + ", " + (_runner.IsRunning == false));

                //Need to check if the task is completed because it might already be done on the ctor and the runner never runs
                return _tcs.Task.IsCompleted || _runner.IsRunning == false;
            }
        }

        public void OnCompleted(Action continuation)
        {
            _awaiter.OnCompleted(continuation);
        }

        public void GetResult()
        {
            _awaiter.GetResult();
        }
    }
}