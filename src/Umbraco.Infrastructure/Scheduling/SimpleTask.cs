using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// A simple task that executes a delegate synchronously
    /// </summary>
    internal class SimpleTask : IBackgroundTask
    {
        private readonly Action _action;

        public SimpleTask(Action action)
        {
            _action = action;
        }

        public bool IsAsync => false;

        public void Run() => _action();

        public Task RunAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
