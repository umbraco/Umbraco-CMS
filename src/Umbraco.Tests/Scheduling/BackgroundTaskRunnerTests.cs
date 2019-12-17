﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Scheduling;

namespace Umbraco.Tests.Scheduling
{
    [TestFixture]
    [Timeout(30000)]
    [Category("Slow")]
    public class BackgroundTaskRunnerTests
    {
        private ILogger _logger;

        [OneTimeSetUp]
        public void InitializeFixture()
        {
            _logger = new ConsoleLogger();
        }

        [Test]
        public async Task ShutdownWhenRunningWithWait()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var stopped = false;
                runner.Stopped += (sender, args) => { stopped = true; };

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask(5000));
                Assert.IsTrue(runner.IsRunning); // is running the task

                runner.Shutdown(false, true); // -force +wait

                // all this before we await because +wait
                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsFalse(runner.IsRunning); // no more running tasks
                Assert.IsTrue(stopped);

                await runner.StoppedAwaitable; // runner stops, within test's timeout
            }
        }

        [Test]
        public async Task ShutdownWhenRunningWithoutWait()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var stopped = false;
                runner.Stopped += (sender, args) => { stopped = true; };

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask(5000));
                Assert.IsTrue(runner.IsRunning); // is running the task

                runner.Shutdown(false, false); // -force +wait

                // all this before we await because -wait
                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsTrue(runner.IsRunning); // still running the task
                Assert.IsFalse(stopped);

                await runner.StoppedAwaitable; // runner stops, within test's timeout

                // and then...
                Assert.IsFalse(runner.IsRunning); // no more running tasks
                Assert.IsTrue(stopped);
            }
        }

        [Test]
        public async Task ShutdownCompletesTheRunner()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Assert.IsFalse(runner.IsRunning); // because AutoStart is false

                // shutdown -force => run all queued tasks
                runner.Shutdown(false, false); // -force -wait
                await runner.StoppedAwaitable; // runner stops, within test's timeout

                Assert.IsFalse(runner.IsRunning); // still not running anything
                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner

                // cannot add tasks to it anymore
                Assert.IsFalse(runner.TryAdd(new MyTask()));
                Assert.Throws<InvalidOperationException>(() =>
                {
                    runner.Add(new MyTask());
                });
            }
        }

        [Test]
        public async Task ShutdownFlushesTheQueue()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                MyTask t1, t2, t3;

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(t1 = new MyTask(5000));
                runner.Add(t2 = new MyTask());
                runner.Add(t3 = new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running tasks

                // shutdown -force => run all queued tasks
                runner.Shutdown(false, false); // -force -wait
                Assert.IsTrue(runner.IsRunning); // is running tasks
                await runner.StoppedAwaitable; // runner stops, within test's timeout

                Assert.AreNotEqual(DateTime.MinValue, t3.Ended); // t3 has run
            }
        }

        [Test]
        public async Task ShutdownForceTruncatesTheQueue()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                MyTask t1, t2, t3;

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(t1 = new MyTask(5000));
                runner.Add(t2 = new MyTask());
                runner.Add(t3 = new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running tasks

                Thread.Sleep(1000); // since we are forcing shutdown, we need to give it a chance to start, else it will be canceled before the queue is started

                // shutdown +force => tries to cancel the current task, ignores queued tasks
                runner.Shutdown(true, false); // +force -wait
                Assert.IsTrue(runner.IsRunning); // is running that long task it cannot cancel

                await runner.StoppedAwaitable; // runner stops, within test's timeout (no cancelation token used, no need to catch OperationCanceledException)

                Assert.AreNotEqual(DateTime.MinValue, t1.Ended); // t1 *has* run
                Assert.AreEqual(DateTime.MinValue, t2.Ended); // t2 has *not* run
                Assert.AreEqual(DateTime.MinValue, t3.Ended); // t3 has *not* run
            }
        }

        [Test]
        public async Task ShutdownThenForce()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                runner.Add(new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running tasks

                // shutdown -force => run all queued tasks
                runner.Shutdown(false, false); // -force -wait

                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsTrue(runner.IsRunning); // still running a task
                Thread.Sleep(3000);
                Assert.IsTrue(runner.IsRunning); // still running a task

                // shutdown +force => tries to cancel the current task, ignores queued tasks
                runner.Shutdown(true, false); // +force -wait
                try
                {
                    await runner.StoppedAwaitable; // runner stops, within test's timeout ... maybe
                }
                catch (OperationCanceledException)
                {
                    // catch exception, this can occur because we are +force shutting down which will
                    // cancel a pending task if the queue hasn't completed in time
                }
            }
        }


        [Test]
        public async Task HostingStopNonImmediate()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                MyTask t;

                var stopped = false;
                runner.Stopped += (sender, args) => { stopped = true; };
                var terminating = false;
                runner.Terminating += (sender, args) => { terminating = true; };
                var terminated = false;
                runner.Terminated += (sender, args) => { terminated = true; };

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask()); // sleeps 500 ms
                runner.Add(new MyTask());  // sleeps 500 ms
                runner.Add(t = new MyTask());  // sleeps 500 ms ... total = 1500 ms until it's done
                Assert.IsTrue(runner.IsRunning); // is running the task

                runner.Stop(false); // -immediate = -force, -wait (max 2000 ms delay before +immediate)
                await runner.TerminatedAwaitable;

                Assert.IsTrue(stopped); // raised that one
                Assert.IsTrue(terminating); // has raised that event
                Assert.IsTrue(terminated);  // and that event

                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsFalse(runner.IsRunning); // done running

                Assert.AreNotEqual(DateTime.MinValue, t.Ended); // t has run
            }
        }

        [Test]
        public async Task HostingStopImmediate()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                MyTask t;

                var stopped = false;
                runner.Stopped += (sender, args) => { stopped = true; };
                var terminating = false;
                runner.Terminating += (sender, args) => { terminating = true; };
                var terminated = false;
                runner.Terminated += (sender, args) => { terminated = true; };

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask()); // sleeps 500 ms
                runner.Add(new MyTask());  // sleeps 500 ms
                runner.Add(t = new MyTask());  // sleeps 500 ms ... total = 1500 ms until it's done
                Assert.IsTrue(runner.IsRunning); // is running the task

                runner.Stop(true); // +immediate = +force, +wait (no delay)
                await runner.TerminatedAwaitable;

                Assert.IsTrue(stopped); // raised that one
                Assert.IsTrue(terminating); // has raised that event
                Assert.IsTrue(terminated); // and that event
                
                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsFalse(runner.IsRunning); // done running

                Assert.AreEqual(DateTime.MinValue, t.Ended); // t has *not* run
            }
        }


        [Test]
        public void Create_IsNotRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Assert.IsFalse(runner.IsRunning);
            }
        }


        [Test]
        public async Task Create_AutoStart_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions
            {
                AutoStart = true,
                KeepAlive = true // else stops!
            }, _logger))
            {
                Assert.IsTrue(runner.IsRunning); // because AutoStart is true
                await runner.StopInternal(false); // keepalive = must be stopped
            }
        }

        [Test]
        public void Create_AutoStartAndKeepAlive_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { AutoStart = true, KeepAlive = true }, _logger))
            {
                Assert.IsTrue(runner.IsRunning); // because AutoStart is true
                Thread.Sleep(800); // for long
                Assert.IsTrue(runner.IsRunning);
                // dispose will stop it
            }
        }

        [Test]
        public async Task Dispose_IsRunning()
        {
            BackgroundTaskRunner<IBackgroundTask> runner;
            using (runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { AutoStart = true, KeepAlive = true }, _logger))
            {
                Assert.IsTrue(runner.IsRunning);
                // dispose will stop it
            }

            try
            {
                await runner.StoppedAwaitable;
            }
            catch (OperationCanceledException)
            {
                // swallow this exception, it can be expected to throw since when disposing we are calling Shutdown +force
                // which depending on a timing operation may cancel the cancelation token
            }


            Assert.Throws<InvalidOperationException>(() => runner.Add(new MyTask()));

        }

        [Test]
        public void Startup_KeepAlive_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { KeepAlive = true }, _logger))
            {
                Assert.IsFalse(runner.IsRunning);
                runner.StartUp();
                Assert.IsTrue(runner.IsRunning);
                // dispose will stop it
            }
        }

        [Test]
        public async Task Create_AddTask_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var waitHandle = new ManualResetEvent(false);
                runner.TaskCompleted += (sender, args) =>
                {
                    waitHandle.Set();
                };
                runner.Add(new MyTask());
                Assert.IsTrue(runner.IsRunning);
                waitHandle.WaitOne();
                await runner.StoppedAwaitable; //since we are not being kept alive, it will quit
                Assert.IsFalse(runner.IsRunning);
            }
        }

        [Test]
        public void Create_KeepAliveAndAddTask_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions { KeepAlive = true }, _logger))
            {
                var waitHandle = new ManualResetEvent(false);
                runner.TaskCompleted += (sender, args) =>
                {
                    Assert.IsTrue(sender.IsRunning);
                    waitHandle.Set();
                };
                runner.Add(new MyTask());
                waitHandle.WaitOne();
                Thread.Sleep(1000); // we are waiting a second just to prove that it's still running and hasn't been shut off
                Assert.IsTrue(runner.IsRunning);
            }
        }

        [Test]
        public async Task WaitOnRunner_OneTask()
        {
            using (var runner = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyTask();
                Assert.IsTrue(task.Ended == default(DateTime));
                runner.Add(task);
                await runner.CurrentThreadingTask;   // wait for the Task operation to complete
                Assert.IsTrue(task.Ended != default(DateTime)); // task is done
                await runner.StoppedAwaitable;               // wait for the entire runner operation to complete
            }
        }


        [Test]
        public async Task WaitOnRunner_Tasks()
        {
            var tasks = new List<BaseTask>();
            for (var i = 0; i < 10; i++)
                tasks.Add(new MyTask());

            using (var runner = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions { KeepAlive = false, LongRunning = true, PreserveRunningTask = true }, _logger))
            {
                tasks.ForEach(runner.Add);

                await runner.StoppedAwaitable; // wait for the entire runner operation to complete

                // check that tasks are done
                Assert.IsTrue(tasks.All(x => x.Ended != default(DateTime)));

                Assert.AreEqual(TaskStatus.RanToCompletion, runner.CurrentThreadingTask.Status);
                Assert.IsFalse(runner.IsRunning);
                Assert.IsFalse(runner.IsDisposed);
            }
        }

        [Test]
        public async Task WaitOnTask()
        {
            using (var runner = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyTask();
                var waitHandle = new ManualResetEvent(false);
                runner.TaskCompleted += (sender, t) => waitHandle.Set();
                Assert.IsTrue(task.Ended == default(DateTime));
                runner.Add(task);
                waitHandle.WaitOne(); // wait 'til task is done
                Assert.IsTrue(task.Ended != default(DateTime)); // task is done
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public async Task WaitOnTasks()
        {
            var tasks = new Dictionary<BaseTask, ManualResetEvent>();
            for (var i = 0; i < 10; i++)
                tasks.Add(new MyTask(), new ManualResetEvent(false));

            using (var runner = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                runner.TaskCompleted += (sender, task) => tasks[task.Task].Set();
                foreach (var t in tasks) runner.Add(t.Key);

                // wait 'til tasks are done, check that tasks are done
                WaitHandle.WaitAll(tasks.Values.Select(x => (WaitHandle)x).ToArray());
                Assert.IsTrue(tasks.All(x => x.Key.Ended != default(DateTime)));

                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public void Tasks_Can_Keep_Being_Added_And_Will_Execute()
        {
            Func<Dictionary<BaseTask, ManualResetEvent>> getTasks = () =>
            {
                var newTasks = new Dictionary<BaseTask, ManualResetEvent>();
                for (var i = 0; i < 10; i++)
                {
                    newTasks.Add(new MyTask(), new ManualResetEvent(false));
                }
                return newTasks;
            };

            IDictionary<BaseTask, ManualResetEvent> tasks = getTasks();

            BackgroundTaskRunner<BaseTask> tManager;
            using (tManager = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions { LongRunning = true, KeepAlive = true }, _logger))
            {
                tManager.TaskCompleted += (sender, task) => tasks[task.Task].Set();

                //execute first batch
                foreach (var t in tasks) tManager.Add(t.Key);

                //wait for all ITasks to complete
                WaitHandle.WaitAll(tasks.Values.Select(x => (WaitHandle)x).ToArray());

                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.Key.Ended != default(DateTime));
                }

                //execute another batch after a bit
                Thread.Sleep(2000);

                tasks = getTasks();
                foreach (var t in tasks) tManager.Add(t.Key);

                //wait for all ITasks to complete
                WaitHandle.WaitAll(tasks.Values.Select(x => (WaitHandle)x).ToArray());

                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.Key.Ended != default(DateTime));
                }
            }
        }

        [Test]
        public async Task Non_Persistent_Runner_Will_Start_New_Threads_When_Required()
        {
            Func<List<BaseTask>> getTasks = () =>
            {
                var newTasks = new List<BaseTask>();
                for (var i = 0; i < 10; i++)
                {
                    newTasks.Add(new MyTask());
                }
                return newTasks;
            };

            List<BaseTask> tasks = getTasks();

            using (var tManager = new BackgroundTaskRunner<BaseTask>(new BackgroundTaskRunnerOptions { LongRunning = true, PreserveRunningTask = true }, _logger))
            {
                tasks.ForEach(tManager.Add);

                //wait till the thread is done
                await tManager.CurrentThreadingTask;

                Assert.AreEqual(TaskStatus.RanToCompletion, tManager.CurrentThreadingTask.Status);
                Assert.IsFalse(tManager.IsRunning);
                Assert.IsFalse(tManager.IsDisposed);

                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.Ended != default(DateTime));
                }

                //create more tasks
                tasks = getTasks();

                //add more tasks
                tasks.ForEach(tManager.Add);

                //wait till the thread is done
                await tManager.CurrentThreadingTask;

                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.Ended != default(DateTime));
                }

                Assert.AreEqual(TaskStatus.RanToCompletion, tManager.CurrentThreadingTask.Status);
                Assert.IsFalse(tManager.IsRunning);
                Assert.IsFalse(tManager.IsDisposed);
            }
        }


        [Test]
        public void RecurringTaskTest()
        {
            var runCount = 0;
            var waitHandle = new ManualResetEvent(false);
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                runner.TaskCompleted += (sender, args) =>
                {
                    runCount++;
                    if (runCount > 3)
                        waitHandle.Set();
                };

                var task = new MyRecurringTask(runner, 200, 500);

                runner.Add(task);

                Assert.IsTrue(runner.IsRunning); // waiting on delay
                Assert.AreEqual(0, runCount);

                waitHandle.WaitOne();

                Assert.GreaterOrEqual(runCount, 4);

                // stops recurring
                runner.Shutdown(false, true);

                // check that task has been disposed (timer has been killed, etc)
                Assert.IsTrue(task.Disposed);
            }
        }

        [Test]
        public async Task LatchedTaskRuns()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyLatchedTask(200, false);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(1000);
                Assert.IsTrue(runner.IsRunning); // still waiting for the task to release
                Assert.IsFalse(task.HasRun);
                task.Release(); // unlatch
                var runnerTask = runner.CurrentThreadingTask; // may be null if things go fast enough
                if (runnerTask != null)
                    await runnerTask; // wait for current task to complete
                Assert.IsTrue(task.HasRun);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public async Task LatchedTaskStops_Runs_On_Shutdown()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyLatchedTask(200, true);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(5000);
                Assert.IsTrue(runner.IsRunning); // still waiting for the task to release
                Assert.IsFalse(task.HasRun);
                runner.Shutdown(false, false); // -force, -wait
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
                Assert.IsTrue(task.HasRun);
            }
        }


        [Test]
        public void LatchedRecurring()
        {
            var runCount = 0;
            var waitHandle = new ManualResetEvent(false);
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                runner.TaskCompleted += (sender, args) =>
                {
                    runCount++;
                    if (runCount > 3)
                        waitHandle.Set();

                };

                var task = new MyDelayedRecurringTask(runner, 2000, 1000);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning); // waiting on delay
                Assert.AreEqual(0, runCount);

                waitHandle.WaitOne();
                Assert.GreaterOrEqual(runCount, 4);
                Assert.IsTrue(task.HasRun);

                // stops recurring
                runner.Shutdown(false, false);
            }
        }

        [Test]
        public async Task FailingTaskSync()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var exceptions = new ConcurrentQueue<Exception>();
                runner.TaskError += (sender, args) => exceptions.Enqueue(args.Exception);

                var task = new MyFailingTask(false, true, false); // -async, +running, -disposing
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete

                Assert.AreEqual(1, exceptions.Count); // traced and reported
            }
        }

        [Test]
        public async Task FailingTaskDisposing()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var exceptions = new ConcurrentQueue<Exception>();
                runner.TaskError += (sender, args) => exceptions.Enqueue(args.Exception);

                var task = new MyFailingTask(false, false, true); // -async, -running, +disposing
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete

                Assert.AreEqual(1, exceptions.Count); // traced and reported
            }
        }

        [Test]
        public async Task FailingTaskAsync()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var exceptions = new ConcurrentQueue<Exception>();
                runner.TaskError += (sender, args) => exceptions.Enqueue(args.Exception);

                var task = new MyFailingTask(true, true, false); // +async, +running, -disposing
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
                Assert.AreEqual(1, exceptions.Count); // traced and reported
            }
        }

        [Test]
        public async Task FailingTaskDisposingAsync()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var exceptions = new ConcurrentQueue<Exception>();
                runner.TaskError += (sender, args) => exceptions.Enqueue(args.Exception);

                var task = new MyFailingTask(false, false, true); // -async, -running, +disposing
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete

                Assert.AreEqual(1, exceptions.Count); // traced and reported
            }
        }

        [Test]
        public async Task CancelAsyncTask()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyAsyncTask(4000);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await Task.Delay(1000); // ensure the task *has* started else cannot cancel
                runner.CancelCurrentBackgroundTask();

                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
                Assert.AreEqual(default(DateTime), task.Ended);
            }
        }

        [Test]
        public async Task CancelLatchedTask()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyLatchedTask(4000, false);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await Task.Delay(1000); // ensure the task *has* started else cannot cancel
                runner.CancelCurrentBackgroundTask();

                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
                Assert.IsFalse(task.HasRun);
            }
        }

        private class MyFailingTask : IBackgroundTask
        {
            private readonly bool _isAsync;
            private readonly bool _running;
            private readonly bool _disposing;

            public MyFailingTask(bool isAsync, bool running, bool disposing)
            {
                _isAsync = isAsync;
                _running = running;
                _disposing = disposing;
            }

            public void Run()
            {
                Thread.Sleep(1000);
                if (_running)
                    throw new Exception("Task has thrown.");
            }

            public async Task RunAsync(CancellationToken token)
            {
                await Task.Delay(1000, token);
                if (_running)
                    throw new Exception("Task has thrown.");
            }

            public bool IsAsync
            {
                get { return _isAsync; }
            }

            public void Dispose()
            {
                if (_disposing)
                    throw new Exception("Task has thrown.");
            }
        }

        private class MyDelayedRecurringTask : RecurringTaskBase
        {
            public bool HasRun { get; private set; }

            public MyDelayedRecurringTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds)
                : base(runner, delayMilliseconds, periodMilliseconds)
            { }

            public override bool IsAsync
            {
                get { return false; }
            }

            public override bool PerformRun()
            {
                HasRun = true;
                return true; // repeat
            }

            public override Task<bool> PerformRunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            public override bool RunsOnShutdown { get { return true; } }
        }

        private class MyLatchedTask : ILatchedBackgroundTask
        {
            private readonly int _runMilliseconds;
            private readonly TaskCompletionSource<bool> _latch;

            public bool HasRun { get; private set; }

            public MyLatchedTask(int runMilliseconds, bool runsOnShutdown)
            {
                _runMilliseconds = runMilliseconds;
                _latch = new TaskCompletionSource<bool>();
                RunsOnShutdown = runsOnShutdown;
            }

            public Task Latch
            {
                get { return _latch.Task; }
            }

            public bool IsLatched
            {
                get { return _latch.Task.IsCompleted == false; }
            }

            public bool RunsOnShutdown { get; private set; }

            public void Run()
            {
                Thread.Sleep(_runMilliseconds);
                HasRun = true;
            }

            public void Release()
            {
                _latch.SetResult(true);
            }

            public Task RunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            public bool IsAsync
            {
                get { return false; }
            }

            public void Dispose()
            { }
        }

        private class MyRecurringTask : RecurringTaskBase
        {
            private readonly int _runMilliseconds;

            public MyRecurringTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int runMilliseconds, int periodMilliseconds)
                : base(runner, 0, periodMilliseconds)
            {
                _runMilliseconds = runMilliseconds;
            }

            public override bool PerformRun()
            {
                Thread.Sleep(_runMilliseconds);
                return true; // repeat
            }

            public override Task<bool> PerformRunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            public override bool IsAsync
            {
                get { return false; }
            }

            public override bool RunsOnShutdown { get { return false; } }
        }

        private class MyTask : BaseTask
        {
            private readonly int _milliseconds;

            public MyTask()
                : this(500)
            { }

            public MyTask(int milliseconds)
            {
                _milliseconds = milliseconds;
            }

            public override void PerformRun()
            {
                Console.WriteLine($"Sleeping {_milliseconds}...");
                Thread.Sleep(_milliseconds);
                Console.WriteLine("Wake up!");
            }
        }

        private class MyAsyncTask : BaseTask
        {
            private readonly int _milliseconds;

            public MyAsyncTask()
                : this(500)
            { }

            public MyAsyncTask(int milliseconds)
            {
                _milliseconds = milliseconds;
            }

            public override void PerformRun()
            {
                throw new NotImplementedException();
            }

            public override async Task RunAsync(CancellationToken token)
            {
                await Task.Delay(_milliseconds, token);
                Ended = DateTime.Now;
            }

            public override bool IsAsync
            {
                get { return true; }
            }
        }

        [Test]
        public void SourceTaskTest()
        {
            var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { KeepAlive = true, LongRunning = true }, _logger);

            var task = new SourceTask();
            runner.Add(task);
            Assert.IsTrue(runner.IsRunning);
            Console.WriteLine("completing");
            task.Complete(); // in Deploy this does not return ffs - no point until I cannot repro
            Console.WriteLine("completed");
            Console.WriteLine("done");
        }

        private class SourceTask : IBackgroundTask
        {
            private readonly SemaphoreSlim _timeout = new SemaphoreSlim(0, 1);
            private readonly TaskCompletionSource<object> _source = new TaskCompletionSource<object>();

            public void Complete()
            {
                _source.SetResult(null);
            }

            public void Dispose()
            { }

            public void Run()
            {
                throw new NotImplementedException();
            }

            public async Task RunAsync(CancellationToken token)
            {
                Console.WriteLine("boom");
                var timeout = _timeout.WaitAsync(token);
                var task = WorkItemRunAsync();
                var anyTask = await Task.WhenAny(task, timeout).ConfigureAwait(false);
            }

            private async Task WorkItemRunAsync()
            {
                await _source.Task.ConfigureAwait(false);
            }

            public bool IsAsync { get { return true; } }
        }

        public abstract class BaseTask : IBackgroundTask
        {
            public bool WasCancelled { get; set; }

            public Guid UniqueId { get; protected set; }

            public abstract void PerformRun();

            public void Run()
            {
                PerformRun();
                Ended = DateTime.Now;
            }

            public virtual Task RunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
                //return Task.Delay(500);
            }

            public virtual bool IsAsync
            {
                get { return false; }
            }

            public virtual void Cancel()
            {
                WasCancelled = true;
            }

            public DateTime Queued { get; set; }
            public DateTime Started { get; set; }
            public DateTime Ended { get; set; }

            public virtual void Dispose()
            {

            }
        }
    }
}
