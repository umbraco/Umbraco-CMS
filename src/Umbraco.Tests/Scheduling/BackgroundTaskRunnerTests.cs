using System;
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
    public class BackgroundTaskRunnerTests
    {
        ILogger _logger; 

        [TestFixtureSetUp]
        public void InitializeFixture()
        {
            _logger = new DebugDiagnosticsLogger();
        }

        [Test]
        public async void ShutdownWhenRunningWithWait()
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
        public async void ShutdownWhenRunningWithoutWait()
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
        public async void ShutdownCompletesTheRunner()
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
        public async void ShutdownFlushesTheQueue()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                MyTask t;

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                runner.Add(t = new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running tasks

                // shutdown -force => run all queued tasks
                runner.Shutdown(false, false); // -force -wait
                Assert.IsTrue(runner.IsRunning); // is running tasks
                await runner.StoppedAwaitable; // runner stops, within test's timeout

                Assert.AreNotEqual(DateTime.MinValue, t.Ended); // t has run
            }
        }

        [Test]
        public async void ShutdownForceTruncatesTheQueue()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                MyTask t;

                Assert.IsFalse(runner.IsRunning); // because AutoStart is false
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                runner.Add(t = new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running tasks

                // shutdown +force => tries to cancel the current task, ignores queued tasks
                runner.Shutdown(true, false); // +force -wait
                Assert.IsTrue(runner.IsRunning); // is running that long task it cannot cancel
                await runner.StoppedAwaitable; // runner stops, within test's timeout

                Assert.AreEqual(DateTime.MinValue, t.Ended); // t has *not* run
            }
        }

        [Test]
        public async void ShutdownThenForce()
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
                await runner.StoppedAwaitable; // runner stops, within test's timeout
            }
        }


        [Test]
        public async void HostingStopNonImmediate()
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
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                runner.Add(t = new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running the task

                runner.Stop(false); // -immediate = -force, -wait
                Assert.IsTrue(terminating); // has raised that event
                Assert.IsFalse(terminated); // but not terminated yet

                // all this before we await because -wait
                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsTrue(runner.IsRunning); // still running the task

                await runner.StoppedAwaitable; // runner stops, within test's timeout
                Assert.IsFalse(runner.IsRunning);
                Assert.IsTrue(stopped);

                await runner.TerminatedAwaitable; // runner terminates, within test's timeout
                Assert.IsTrue(terminated); // has raised that event

                Assert.AreNotEqual(DateTime.MinValue, t.Ended); // t has run
            }
        }

        [Test]
        public async void HostingStopImmediate()
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
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                runner.Add(t = new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running the task

                runner.Stop(true); // +immediate = +force, +wait
                Assert.IsTrue(terminating); // has raised that event
                Assert.IsTrue(terminated); // and that event
                Assert.IsTrue(stopped); // and that one

                // and all this before we await because +wait
                Assert.IsTrue(runner.IsCompleted); // shutdown completes the runner
                Assert.IsFalse(runner.IsRunning); // done running

                await runner.StoppedAwaitable; // runner stops, within test's timeout
                await runner.TerminatedAwaitable; // runner terminates, within test's timeout

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
        public async void Create_AutoStart_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions
            {
                AutoStart = true
            }, _logger))
            {
                Assert.IsTrue(runner.IsRunning); // because AutoStart is true
                await runner.StoppedAwaitable; // runner stops, within test's timeout
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
        public async void Dispose_IsRunning()
        {
            BackgroundTaskRunner<IBackgroundTask> runner;
            using (runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { AutoStart = true, KeepAlive = true }, _logger))
            {
                Assert.IsTrue(runner.IsRunning);
                // dispose will stop it
            }

            await runner.StoppedAwaitable; // runner stops, within test's timeout
            //await runner.TerminatedAwaitable; // NO! see note below
            Assert.Throws<InvalidOperationException>(() => runner.Add(new MyTask()));

            // but do NOT await on TerminatedAwaitable - disposing just shuts the runner down
            // so that we don't have a runaway task in tests, etc - but it does NOT terminate
            // the runner - it really is NOT a nice way to end a runner - it's there for tests
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
        public async void Create_AddTask_IsRunning()
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
        public async void WaitOnRunner_OneTask()
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
        public async void WaitOnRunner_Tasks()
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
        public async void WaitOnTask()
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
        public async void WaitOnTasks()
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
                tasks.ForEach(t => tManager.Add(t.Key));

                //wait for all ITasks to complete
                WaitHandle.WaitAll(tasks.Values.Select(x => (WaitHandle)x).ToArray());

                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.Key.Ended != default(DateTime));
                }

                //execute another batch after a bit
                Thread.Sleep(2000);

                tasks = getTasks();
                tasks.ForEach(t => tManager.Add(t.Key));

                //wait for all ITasks to complete
                WaitHandle.WaitAll(tasks.Values.Select(x => (WaitHandle)x).ToArray());

                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.Key.Ended != default(DateTime));
                }
            }
        }

        [Test]
        public async void Non_Persistent_Runner_Will_Start_New_Threads_When_Required()
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
                runner.TaskCompleted += (sender, args) => runCount++;
                runner.TaskStarting += async (sender, args) =>
                {
                    //wait for each task to finish once it's started
                    await sender.CurrentThreadingTask;
                    if (runCount > 3)
                    {
                        waitHandle.Set();
                    }
                };

                var task = new MyRecurringTask(runner, 200, 500);
                
                runner.Add(task);

                Assert.IsTrue(runner.IsRunning); // waiting on delay
                Assert.AreEqual(0, runCount);

                waitHandle.WaitOne();

                Assert.GreaterOrEqual(runCount, 4);

                // stops recurring
                runner.Shutdown(false, false);
            }
        }

        [Test]
        public async void DelayedTaskRuns()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyDelayedTask(200, false);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(5000);
                Assert.IsTrue(runner.IsRunning); // still waiting for the task to release
                Assert.IsFalse(task.HasRun);
                task.Release();
                await runner.CurrentThreadingTask; //wait for current task to complete
                Assert.IsTrue(task.HasRun);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public async void DelayedTaskStops()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyDelayedTask(200, true);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(5000);
                Assert.IsTrue(runner.IsRunning); // still waiting for the task to release               
                Assert.IsFalse(task.HasRun);
                runner.Shutdown(false, false);
                await runner.StoppedAwaitable; // wait for the entire runner operation to complete
                Assert.IsTrue(task.HasRun);
            }
        }

        
        [Test]
        public void DelayedRecurring()
        {
            var runCount = 0;
            var waitHandle = new ManualResetEvent(false);
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                runner.TaskCompleted += (sender, args) => runCount++;
                runner.TaskStarting += async (sender, args) =>
                {
                    //wait for each task to finish once it's started
                    await sender.CurrentThreadingTask;
                    if (runCount > 3)
                    {
                        waitHandle.Set();
                    }
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
        public async void FailingTaskSync()
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
        public async void FailingTaskDisposing()
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
        public async void FailingTaskAsync()
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
        public async void FailingTaskDisposingAsync()
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
                await Task.Delay(1000);
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

        private class MyDelayedRecurringTask : DelayedRecurringTaskBase<MyDelayedRecurringTask>
        {
            public bool HasRun { get; private set; }

            public MyDelayedRecurringTask(IBackgroundTaskRunner<MyDelayedRecurringTask> runner, int delayMilliseconds, int periodMilliseconds)
                : base(runner, delayMilliseconds, periodMilliseconds)
            { }

            private MyDelayedRecurringTask(MyDelayedRecurringTask source)
                : base(source)
            { }

            public override bool IsAsync
            {
                get { return false; }
            }

            public override void PerformRun()
            {
                HasRun = true;
            }

            public override Task PerformRunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            protected override MyDelayedRecurringTask GetRecurring()
            {
                return new MyDelayedRecurringTask(this);
            }
        }

        private class MyDelayedTask : ILatchedBackgroundTask
        {
            private readonly int _runMilliseconds;
            private readonly ManualResetEventSlim _gate;

            public bool HasRun { get; private set; }

            public MyDelayedTask(int runMilliseconds, bool runsOnShutdown)
            {
                _runMilliseconds = runMilliseconds;
                _gate = new ManualResetEventSlim(false);
                RunsOnShutdown = runsOnShutdown;
            }

            public WaitHandle Latch
            {
                get { return _gate.WaitHandle; }
            }

            public bool IsLatched
            {
                get { return _gate.IsSet == false; }
            }

            public bool RunsOnShutdown { get; private set; }

            public void Run()
            {
                Thread.Sleep(_runMilliseconds);
                HasRun = true;
            }

            public void Release()
            {
                _gate.Set();
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

        private class MyRecurringTask : RecurringTaskBase<MyRecurringTask>
        {
            private readonly int _runMilliseconds;


            public MyRecurringTask(IBackgroundTaskRunner<MyRecurringTask> runner, int runMilliseconds, int periodMilliseconds)
                : base(runner, periodMilliseconds)
            {
                _runMilliseconds = runMilliseconds;
            }

            private MyRecurringTask(MyRecurringTask source, int runMilliseconds)
                : base(source)
            {
                _runMilliseconds = runMilliseconds;
            }

            public override void PerformRun()
            {                
                Thread.Sleep(_runMilliseconds);
            }

            public override Task PerformRunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            public override bool IsAsync
            {
                get { return false; }
            }

            protected override MyRecurringTask GetRecurring()
            {
                return new MyRecurringTask(this, _runMilliseconds);
            }
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
                Thread.Sleep(_milliseconds);
            }
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

            public Task RunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
                //return Task.Delay(500); 
            }

            public bool IsAsync
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
            { }
        }
    }
}
