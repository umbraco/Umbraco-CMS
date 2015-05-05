using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Events;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Manages a queue of tasks of type <typeparamref name="T"/> and runs them in the background.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    /// <remarks>The task runner is web-aware and will ensure that it shuts down correctly when the AppDomain
    /// shuts down (ie is unloaded).</remarks>
    internal class BackgroundTaskRunner<T> : IBackgroundTaskRunner<T>
        where T : class, IBackgroundTask
    {
        private readonly BackgroundTaskRunnerOptions _options;
        private readonly ILogger _logger;
        private readonly BlockingCollection<T> _tasks = new BlockingCollection<T>();
        private readonly object _locker = new object();
        private readonly ManualResetEventSlim _completedEvent = new ManualResetEventSlim(false);
        private BackgroundTaskRunnerAwaiter<T> _awaiter;

        private volatile bool _isRunning; // is running
        private volatile bool _isCompleted; // does not accept tasks anymore, may still be running
        private Task _runningTask;

        private CancellationTokenSource _tokenSource;

        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskError;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskStarting;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskCompleted;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskCancelled;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Completed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        public BackgroundTaskRunner(ILogger logger)
            : this(new BackgroundTaskRunnerOptions(), logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="options">The set of options.</param>
        /// <param name="logger"></param>
        public BackgroundTaskRunner(BackgroundTaskRunnerOptions options, ILogger logger)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (logger == null) throw new ArgumentNullException("logger");
            _options = options;
            _logger = logger;

            HostingEnvironment.RegisterObject(this);

            if (options.AutoStart)
                StartUp();
        }

        /// <summary>
        /// Gets the number of tasks in the queue.
        /// </summary>
        public int TaskCount
        {
            get { return _tasks.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether a task is currently running.
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
        }

        /// <summary>
        /// Gets a value indicating whether the runner has completed and cannot accept tasks anymore.
        /// </summary>
        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        /// <summary>
        /// Gets an awaiter used to await the running Threading.Task.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no running task.</exception>
        /// <remarks>
        /// Unless the AutoStart option is true, there will be no running task until
        /// a background task is added to the queue. Unless the KeepAlive option is true, there
        /// will be no running task when the queue is empty.
        /// </remarks>
        public ThreadingTaskAwaiter CurrentThreadingTask
        {
            get
            {
                if (_runningTask == null)
                    throw new InvalidOperationException("There is no current Threading.Task.");
                return new ThreadingTaskAwaiter(_runningTask);
            }
        }

        /// <summary>
        /// Gets an awaiter used to await the BackgroundTaskRunner running operation
        /// </summary>
        /// <returns>An awaiter for the BackgroundTaskRunner running operation</returns>
        /// <remarks>
        /// <para>This is used to wait until the background task runner is no longer running (IsRunning == false)
        /// </para>
        /// <para> So long as we have a method called GetAwaiter() that returns an instance of INotifyCompletion 
        /// we can await anything. In this case we are awaiting with a custom BackgroundTaskRunnerAwaiter
        /// which waits for the Completed event to be raised. 
        /// ref:  http://blogs.msdn.com/b/pfxteam/archive/2011/01/13/10115642.aspx
        /// </para>
        /// </remarks>        
        public BackgroundTaskRunnerAwaiter<T> GetAwaiter()
        {
            return _awaiter ?? (_awaiter = new BackgroundTaskRunnerAwaiter<T>(this, _logger));
        }

        /// <summary>
        /// Adds a task to the queue.
        /// </summary>
        /// <param name="task">The task to add.</param>
        /// <exception cref="InvalidOperationException">The task runner has completed.</exception>
        public void Add(T task)
        {
            lock (_locker)
            {
                if (_isCompleted)
                    throw new InvalidOperationException("The task runner has completed.");

                // add task
                _logger.Debug<BackgroundTaskRunner<T>>("Task added {0}", task.GetType);
                _tasks.Add(task);

                // start
                StartUpLocked();
            }
        }

        /// <summary>
        /// Tries to add a task to the queue.
        /// </summary>
        /// <param name="task">The task to add.</param>
        /// <returns>true if the task could be added to the queue; otherwise false.</returns>
        /// <remarks>Returns false if the runner is completed.</remarks>
        public bool TryAdd(T task)
        {
            lock (_locker)
            {
                if (_isCompleted) return false;

                // add task
                _logger.Debug<BackgroundTaskRunner<T>>("Task added {0}", task.GetType);
                _tasks.Add(task);

                // start
                StartUpLocked();

                return true;
            }
        }

        /// <summary>
        /// Starts the tasks runner, if not already running.
        /// </summary>
        /// <remarks>Is invoked each time a task is added, to ensure it is going to be processed.</remarks>
        /// <exception cref="InvalidOperationException">The task runner has completed.</exception>
        public void StartUp()
        {
            if (_isRunning) return;

            lock (_locker)
            {
                if (_isCompleted)
                    throw new InvalidOperationException("The task runner has completed.");

                StartUpLocked();
            }
        }

        /// <summary>
        /// Starts the tasks runner, if not already running.
        /// </summary>
        /// <remarks>Must be invoked within lock(_locker) and with _isCompleted being false.</remarks>
        private void StartUpLocked()
        {
            // double check 
            if (_isRunning) return;
            _isRunning = true;

            // create a new token source since this is a new process
            _tokenSource = new CancellationTokenSource();
            _runningTask = PumpIBackgroundTasks(Task.Factory, _tokenSource.Token);
            _logger.Debug<BackgroundTaskRunner<T>>("Starting");
        }

        /// <summary>
        /// Shuts the taks runner down.
        /// </summary>
        /// <param name="force">True for force the runner to stop.</param>
        /// <param name="wait">True to wait until the runner has stopped.</param>
        /// <remarks>If <paramref name="force"/> is false, no more tasks can be queued but all queued tasks
        /// will run. If it is true, then only the current one (if any) will end and no other task will run.</remarks>
        public void Shutdown(bool force, bool wait)
        {
            lock (_locker)
            {
                _isCompleted = true; // do not accept new tasks
                if (_isRunning == false) return; // done already
            }

            // try to be nice
            // assuming multiple threads can do these without problems
            _completedEvent.Set();
            _tasks.CompleteAdding();

            if (force)
            {
                // we must bring everything down, now
                Thread.Sleep(100); // give time to CompleAdding()
                lock (_locker)
                {
                    // was CompleteAdding() enough?
                    if (_isRunning == false) return;
                }
                // try to cancel running async tasks (cannot do much about sync tasks)
                // break delayed tasks delay
                // truncate running queues
                _tokenSource.Cancel(false); // false is the default
            }

            // tasks in the queue will be executed...
            if (wait == false) return;
            _runningTask.Wait(); // wait for whatever is running to end...

        }

        /// <summary>
        /// Runs background tasks for as long as there are background tasks in the queue, with an asynchronous operation.
        /// </summary>
        /// <param name="factory">The supporting <see cref="TaskFactory"/>.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>The asynchronous operation.</returns>
        private Task PumpIBackgroundTasks(TaskFactory factory, CancellationToken token)
        {
            var taskSource = new TaskCompletionSource<object>(factory.CreationOptions);
            var enumerator = _options.KeepAlive ? _tasks.GetConsumingEnumerable(token).GetEnumerator() : null;

            // ReSharper disable once MethodSupportsCancellation // always run
            var taskSourceContinuing = taskSource.Task.ContinueWith(t =>
            {
                // because the pump does not lock, there's a race condition,
                // the pump may stop and then we still have tasks to process,
                // and then we must restart the pump - lock to avoid race cond
                lock (_locker)
                {
                    if (token.IsCancellationRequested || _tasks.Count == 0)
                    {
                        _logger.Debug<BackgroundTaskRunner<T>>("_isRunning = false");

                        _isRunning = false; // done
                        if (_options.PreserveRunningTask == false)
                            _runningTask = null;
                        //raise event
                        OnCompleted();
                        return;
                    }
                }

                // if _runningTask is taskSource.Task then we must keep continuing it,
                // not starting a new taskSource, else _runningTask would complete and
                // something may be waiting on it
                //PumpIBackgroundTasks(factory, token); // restart
                // ReSharper disable once MethodSupportsCancellation // always run
                t.ContinueWithTask(_ => PumpIBackgroundTasks(factory, token)); // restart
            });

            Action<Task> pump = null;
            pump = task =>
            {
                // RunIBackgroundTaskAsync does NOT throw exceptions, just raises event
                // so if we have an exception here, really, wtf? - must read the exception
                // anyways so it does not bubble up and kill everything
                if (task != null && task.IsFaulted)
                {
                    var exception = task.Exception;
                    _logger.Error<BackgroundTaskRunner<T>>("Task runner exception.", exception);
                }

                // is it ok to run?
                if (TaskSourceCanceled(taskSource, token)) return;

                // try to get a task
                // the blocking MoveNext will end if token is cancelled or collection is completed
                T bgTask;
                var hasBgTask = _options.KeepAlive
                    ? (bgTask = enumerator.MoveNext() ? enumerator.Current : null) != null // blocking
                    : _tasks.TryTake(out bgTask); // non-blocking

                // no task, signal the runner we're done
                if (hasBgTask == false)
                {
                    TaskSourceCompleted(taskSource, token);
                    return;
                }

                // wait for latched task, supporting cancellation
                var dbgTask = bgTask as ILatchedBackgroundTask;
                if (dbgTask != null && dbgTask.IsLatched)
                {
                    WaitHandle.WaitAny(new[] { dbgTask.Latch, token.WaitHandle, _completedEvent.WaitHandle });
                    if (TaskSourceCanceled(taskSource, token)) return;
                    // else run now, either because latch ok or runner is completed
                    // still latched & not running on shutdown = stop here
                    if (dbgTask.IsLatched && dbgTask.RunsOnShutdown == false)
                    {
                        TaskSourceCompleted(taskSource, token);
                        return;
                    }
                }

                // run the task as first task, or a continuation
                task = task == null 
                    ? RunIBackgroundTaskAsync(bgTask, token)
                    // ReSharper disable once MethodSupportsCancellation // always run
                    : task.ContinueWithTask(_ => RunIBackgroundTaskAsync(bgTask, token));

                // and pump
                // ReSharper disable once MethodSupportsCancellation // always run
                task.ContinueWith(t => pump(t));
            };

            // start it all
            factory.StartNew(() => pump(null),
                token,
                _options.LongRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None,
                TaskScheduler.Default);

            return taskSourceContinuing;
        }

        private bool TaskSourceCanceled(TaskCompletionSource<object> taskSource, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                taskSource.SetCanceled();
                return true;
            }
            return false;
        }

        private void TaskSourceCompleted(TaskCompletionSource<object> taskSource, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                taskSource.SetCanceled();
            else
                taskSource.SetResult(null);
        }

        /// <summary>
        /// Runs a background task asynchronously.
        /// </summary>
        /// <param name="bgTask">The background task.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>The asynchronous operation.</returns>
        internal async Task RunIBackgroundTaskAsync(T bgTask, CancellationToken token)
        {
            try
            {
                OnTaskStarting(new TaskEventArgs<T>(bgTask));

                try
                {
                    using (bgTask) // ensure it's disposed
                    {
                        if (bgTask.IsAsync)
                            //configure await = false since we don't care about the context, we're on a background thread.
                            await bgTask.RunAsync(token).ConfigureAwait(false);
                        else
                            bgTask.Run();
                    }
                }
                catch (Exception e)
                {
                    OnTaskError(new TaskEventArgs<T>(bgTask, e));
                    throw;
                }

                OnTaskCompleted(new TaskEventArgs<T>(bgTask));
            }
            catch (Exception ex)
            {
                _logger.Error<BackgroundTaskRunner<T>>("Task has failed.", ex);
            }            
        }

        #region Events

        protected virtual void OnTaskError(TaskEventArgs<T> e)
        {
            var handler = TaskError;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnTaskStarting(TaskEventArgs<T> e)
        {
            var handler = TaskStarting;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    _logger.Error<BackgroundTaskRunner<T>>("TaskStarting exception occurred", ex);
                }
            }
        }

        protected virtual void OnTaskCompleted(TaskEventArgs<T> e)
        {
            var handler = TaskCompleted;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    _logger.Error<BackgroundTaskRunner<T>>("TaskCompleted exception occurred", ex);
                }
            }
        }

        protected virtual void OnTaskCancelled(TaskEventArgs<T> e)
        {
            var handler = TaskCancelled;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    _logger.Error<BackgroundTaskRunner<T>>("TaskCancelled exception occurred", ex);
                }
            }

            //dispose it
            e.Task.Dispose();
        }

        protected virtual void OnCompleted()
        {
            var handler = Completed;
            if (handler != null)
            {
                try
                {
                    handler(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _logger.Error<BackgroundTaskRunner<T>>("OnCompleted exception occurred", ex);
                }
            }
        }

        #endregion

        #region IDisposable

        private readonly object _disposalLocker = new object();
        public bool IsDisposed { get; private set; }

        ~BackgroundTaskRunner()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || disposing == false)
                return;

            lock (_disposalLocker)
            {
                if (IsDisposed)
                    return;
                DisposeResources();
                IsDisposed = true;
            }
        }

        protected virtual void DisposeResources()
        {
            // just make sure we eventually go down
            Shutdown(true, false);
        }

        #endregion

        /// <summary>
        /// Requests a registered object to unregister.
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should unregister from the hosting
        /// environment before returning; otherwise, false.</param>
        /// <remarks>
        /// <para>"When the application manager needs to stop a registered object, it will call the Stop method."</para>
        /// <para>The application manager will call the Stop method to ask a registered object to unregister. During
        /// processing of the Stop method, the registered object must call the HostingEnvironment.UnregisterObject method.</para>
        /// </remarks>
        public void Stop(bool immediate)
        {
            if (immediate == false)
            {
                // The Stop method is first called with the immediate parameter set to false. The object can either complete
                // processing, call the UnregisterObject method, and then return or it can return immediately and complete
                // processing asynchronously before calling the UnregisterObject method.

                _logger.Debug<BackgroundTaskRunner<T>>("Shutting down, waiting for tasks to complete.");
                Shutdown(false, false); // do not accept any more tasks, flush the queue, do not wait

                lock (_locker)
                {
                    if (_runningTask != null)
                        _runningTask.ContinueWith(_ =>
                        {
                            HostingEnvironment.UnregisterObject(this);
                            _logger.Info<BackgroundTaskRunner<T>>("Down, tasks completed.");
                        });
                    else
                    {
                        HostingEnvironment.UnregisterObject(this);
                        _logger.Info<BackgroundTaskRunner<T>>("Down, tasks completed.");
                    }
                }
            }
            else
            {
                // If the registered object does not complete processing before the application manager's time-out
                // period expires, the Stop method is called again with the immediate parameter set to true. When the
                // immediate parameter is true, the registered object must call the UnregisterObject method before returning;
                // otherwise, its registration will be removed by the application manager.

                _logger.Info<BackgroundTaskRunner<T>>("Shutting down immediately.");
                Shutdown(true, true); // cancel all tasks, wait for the current one to end
                HostingEnvironment.UnregisterObject(this);
                _logger.Info<BackgroundTaskRunner<T>>("Down.");
            }
        }

        
    }
}
