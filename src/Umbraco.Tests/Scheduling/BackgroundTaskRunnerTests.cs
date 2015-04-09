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
        public async void ShutdownWaitWhenRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { AutoStart = true, KeepAlive = true }, _logger))
            {
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(800); // for long
                Assert.IsTrue(runner.IsRunning);
                runner.Shutdown(false, true); // -force +wait
                await runner; // wait for the entire runner operation to complete
                Assert.IsTrue(runner.IsCompleted);
            }
        }

        [Test]
        public async void ShutdownWhenRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Console.WriteLine("Begin {0}", DateTime.Now);

                runner.TaskStarting += (sender, args) => Console.WriteLine("starting {0}", DateTime.Now);
                runner.TaskCompleted += (sender, args) => Console.WriteLine("completed {0}", DateTime.Now);

                Assert.IsFalse(runner.IsRunning);

                Console.WriteLine("Adding task {0}", DateTime.Now);
                runner.Add(new MyTask(5000));
                Assert.IsTrue(runner.IsRunning); // is running the task
                Console.WriteLine("Shutting down {0}", DateTime.Now);
                runner.Shutdown(false, false); // -force -wait
                Assert.IsTrue(runner.IsCompleted);
                Assert.IsTrue(runner.IsRunning); // still running that task
                Thread.Sleep(3000);              // wait slightly less than the task takes to complete
                Assert.IsTrue(runner.IsRunning); // still running that task

                await runner; // wait for the entire runner operation to complete
                
                Console.WriteLine("End {0}", DateTime.Now);
            }
        }

        [Test]
        public async void ShutdownFlushesTheQueue()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Assert.IsFalse(runner.IsRunning);
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                var t = new MyTask();
                runner.Add(t);
                Assert.IsTrue(runner.IsRunning); // is running the first task
                runner.Shutdown(false, false); // -force -wait
                await runner; // wait for the entire runner operation to complete
                Assert.AreNotEqual(DateTime.MinValue, t.Ended); // t has run
            }
        }

        [Test]
        public async void ShutdownForceTruncatesTheQueue()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Assert.IsFalse(runner.IsRunning);
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                var t = new MyTask();
                runner.Add(t);
                Assert.IsTrue(runner.IsRunning); // is running the first task
                runner.Shutdown(true, false); // +force -wait
                await runner; // wait for the entire runner operation to complete
                Assert.AreEqual(DateTime.MinValue, t.Ended); // t has not run
            }
        }

        [Test]
        public async void ShutdownThenForce()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Assert.IsFalse(runner.IsRunning);
                runner.Add(new MyTask(5000));
                runner.Add(new MyTask());
                runner.Add(new MyTask());
                Assert.IsTrue(runner.IsRunning); // is running the task
                runner.Shutdown(false, false); // -force -wait
                Assert.IsTrue(runner.IsCompleted);
                Assert.IsTrue(runner.IsRunning); // still running that task
                Thread.Sleep(3000);
                Assert.IsTrue(runner.IsRunning); // still running that task
                runner.Shutdown(true, false); // +force -wait
                await runner; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public void Create_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                Assert.IsFalse(runner.IsRunning);
            }
        }

        [Test]
        public async void Create_AutoStart_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { AutoStart = true }, _logger))
            {
                Assert.IsTrue(runner.IsRunning);
                await runner; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public void Create_AutoStartAndKeepAlive_IsRunning()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions { AutoStart = true, KeepAlive = true }, _logger))
            {
                Assert.IsTrue(runner.IsRunning);
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

            await runner; // wait for the entire runner operation to complete
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
                await runner; //since we are not being kept alive, it will quit
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
                await runner;               // wait for the entire runner operation to complete
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

                await runner; // wait for the entire runner operation to complete

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
                await runner; // wait for the entire runner operation to complete
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

                await runner; // wait for the entire runner operation to complete
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
                var task = new MyDelayedTask(200);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(5000);
                Assert.IsTrue(runner.IsRunning); // still waiting for the task to release
                Assert.IsFalse(task.HasRun);
                task.Release();
                await runner.CurrentThreadingTask; //wait for current task to complete
                Assert.IsTrue(task.HasRun);
                await runner; // wait for the entire runner operation to complete
            }
        }

        [Test]
        public async void DelayedTaskStops()
        {
            using (var runner = new BackgroundTaskRunner<IBackgroundTask>(new BackgroundTaskRunnerOptions(), _logger))
            {
                var task = new MyDelayedTask(200);
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                Thread.Sleep(5000);
                Assert.IsTrue(runner.IsRunning); // still waiting for the task to release               
                Assert.IsFalse(task.HasRun);
                runner.Shutdown(false, false);
                await runner; // wait for the entire runner operation to complete
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

                var task = new MyFailingTask(false); // -async
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await runner; // wait for the entire runner operation to complete

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

                var task = new MyFailingTask(true); // +async
                runner.Add(task);
                Assert.IsTrue(runner.IsRunning);
                await runner; // wait for the entire runner operation to complete

                Assert.AreEqual(1, exceptions.Count); // traced and reported
            }
        }

        private class MyFailingTask : IBackgroundTask
        {
            private readonly bool _isAsync;

            public MyFailingTask(bool isAsync)
            {
                _isAsync = isAsync;
            }

            public void Run()
            {
                Thread.Sleep(1000);
                throw new Exception("Task has thrown.");
            }

            public async Task RunAsync(CancellationToken token)
            {
                await Task.Delay(1000);
                throw new Exception("Task has thrown.");
            }

            public bool IsAsync
            {
                get { return _isAsync; }
            }

            // fixme - must also test what happens if we throw on dispose!
            public void Dispose()
            { }
        }

        private class MyDelayedRecurringTask : DelayedRecurringTaskBase<MyDelayedRecurringTask>
        {
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
                // nothing to do at the moment
            }

            public override Task PerformRunAsync()
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
            private readonly ManualResetEvent _gate;

            public bool HasRun { get; private set; }

            public MyDelayedTask(int runMilliseconds)
            {
                _runMilliseconds = runMilliseconds;
                _gate = new ManualResetEvent(false);
            }

            public WaitHandle Latch
            {
                get { return _gate; }
            }

            public bool IsLatched
            {
                get { return true; }
            }

            public bool RunsOnShutdown
            {
                get { return true; }
            }

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

            public override Task PerformRunAsync()
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
