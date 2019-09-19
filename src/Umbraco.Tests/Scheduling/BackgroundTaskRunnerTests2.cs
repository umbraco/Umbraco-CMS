using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Scheduling;

namespace Umbraco.Tests.Scheduling
{
    [TestFixture]
    [Timeout(60000)]
    public class BackgroundTaskRunnerTests2
    {
        // this tests was used to debug a background task runner issue that was unearthed by Deploy,
        // where work items would never complete under certain circumstances, due to threading issues.
        // (fixed now)
        //
        [Test]
        [Timeout(4000)]
        public async Task ThreadResumeIssue()
        {
            var logger = new ConsoleLogger();
            var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { KeepAlive = true, LongRunning = true }, logger);
            var work = new ThreadResumeIssueWorkItem();
            runner.Add(work);

            Console.WriteLine("running");
            await Task.Delay(1000); // don't complete too soon

            Console.WriteLine("completing");

            // this never returned, never reached "completed" because the same thread
            // resumed executing the waiting on queue operation in the runner
            work.Complete();
            Console.WriteLine("completed");

            Console.WriteLine("done");
        }

        public class ThreadResumeIssueWorkItem : IBackgroundTask
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
                // this never returned, see test
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

        [Test]
        [Ignore("Only runs in the debugger.")]
        public async Task DebuggerInterferenceIssue()
        {
            var logger = new ConsoleLogger();
            var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { KeepAlive = true, LongRunning = true }, logger);
            var taskCompleted = false;
            runner.TaskCompleted += (sender, args) =>
            {
                Console.WriteLine("runner task completed");
                taskCompleted = true;
            };
            var work = new DebuggerInterferenceIssueWorkitem();

            // add the workitem to the runner and wait until it is running
            runner.Add(work);
            work.Running.Wait();

            // then wait a little bit more to ensure that the WhenAny has been entered
            await Task.Delay(500);

            // then break
            // when the timeout triggers, we cannot handle it
            // taskCompleted value does *not* change & nothing happens
            Debugger.Break();

            // release after 15s
            // WhenAny should return the timeout task
            // and then taskCompleted should turn to true
            // = debugging does not prevent task completion

            Console.WriteLine("*");
            Assert.IsFalse(taskCompleted);
            await Task.Delay(1000);
            Console.WriteLine("*");
            Assert.IsTrue(taskCompleted);
        }

        public class DebuggerInterferenceIssueWorkitem : IBackgroundTask
        {
            private readonly SemaphoreSlim _timeout = new SemaphoreSlim(0, 1);
            private readonly ManualResetEventSlim _running = new ManualResetEventSlim(false);

            private Timer _timer;

            public ManualResetEventSlim Running { get { return _running; } }

            public async Task RunAsync(CancellationToken token)
            {
                // timeout timer
                _timer = new Timer(_ => { _timeout.Release(); });
                _timer.Change(1000, 0);

                var timeout = _timeout.WaitAsync(token);
                var source = CancellationTokenSource.CreateLinkedTokenSource(token); // cancels when token cancels

                _running.Set();
                var task = WorkExecuteAsync(source.Token);
                Console.WriteLine("execute");
                var anyTask = await Task.WhenAny(task, timeout).ConfigureAwait(false);

                Console.Write("anyTask: ");
                Console.WriteLine(anyTask == timeout ? "timeout" : "task");

                Console.WriteLine("return");
            }

            private async Task WorkExecuteAsync(CancellationToken token)
            {
                await Task.Delay(30000);
            }

            public void Run()
            {
                throw new NotImplementedException();
            }

            public bool IsAsync { get { return true; } }

            public void Dispose()
            { }
        }

        [Test]
        [Ignore("Only runs in the debugger.")]
        public void TimerDebuggerTest()
        {
            var triggered = false;
            var timer = new Timer(_ => { triggered = true; });
            timer.Change(1000, 0);
            Debugger.Break();

            // pause in debugger for 10s
            // means the timer triggers while execution is suspended
            // 'triggered' remains false all along
            // then resume execution
            // and 'triggered' becomes true, so the trigger "catches up"
            // = debugging should not prevent triggered code from executing

            Thread.Sleep(200);
            Assert.IsTrue(triggered);
        }
    }
}