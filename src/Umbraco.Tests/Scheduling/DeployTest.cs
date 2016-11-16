using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Tests.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.Tests.Scheduling
{
    // THIS REPRODUCES THE DEPLOY ISSUE IN CORE
    //
    // the exact same thing also reproduces in playground
    // so it's not a framework version issue - but something we're doing here

    [TestFixture]
    [Timeout(4000)]
    public class Repro
    {
        [Test]
        public async Task ReproduceDeployIssue()
        {
            var logger = new ConsoleLogger();
            var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { KeepAlive = true, LongRunning = true }, logger);
            var work = new SimpleWorkItem();
            runner.Add(work);

            Console.WriteLine("running");
            await Task.Delay(1000); // don't complete too soon

            Console.WriteLine("completing");

            // this never returns, never reached "completed" because the same thread
            // resumes executing the waiting on queue operation in the runner
            work.Complete();
            Console.WriteLine("completed");

            Console.WriteLine("done");
        }

        public class SimpleWorkItem : IBackgroundTask
        {
            private TaskCompletionSource<int> _completionSource;

            public async Task RunAsync(CancellationToken token)
            {
                _completionSource = new TaskCompletionSource<int>();
                token.Register(() => _completionSource.TrySetCanceled()); // propagate
                Console.WriteLine("item running...");
                await _completionSource.Task.ConfigureAwait(false);
                Console.WriteLine("item returning");
            }

            public bool Complete(bool success = true)
            {
                Console.WriteLine("item completing");
                // this never returns, see test
                _completionSource.SetResult(0);
                Console.WriteLine("item returning from completing");
                return true;
            }

            public void Run()
            {
                throw new NotImplementedException();
            }

            public bool IsAsync { get { return true; } }

            public void Dispose()
            {}
        }
    }
}