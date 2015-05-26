using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{
    // exists for logging purposes
    internal class BackgroundTaskRunner
    { }

    /// <summary>
    /// Manages a queue of tasks of type <typeparamref name="T"/> and runs them in the background.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    /// <remarks>The task runner is web-aware and will ensure that it shuts down correctly when the AppDomain
    /// shuts down (ie is unloaded).</remarks>
    internal class BackgroundTaskRunner<T> : BackgroundTaskRunner, IBackgroundTaskRunner<T>
        where T : class, IBackgroundTask
    {
        private readonly string _logPrefix;
        private readonly BackgroundTaskRunnerOptions _options;
        private readonly ILogger _logger;
        private readonly BlockingCollection<T> _tasks = new BlockingCollection<T>();
        private readonly object _locker = new object();

        // that event is used to stop the pump when it is alive and waiting
        // on a latched task - so it waits on the latch, the cancellation token,
        // and the completed event
        private readonly ManualResetEventSlim _completedEvent = new ManualResetEventSlim(false);

        // in various places we are testing these vars outside a lock, so make them volatile
        private volatile bool _isRunning; // is running
        private volatile bool _isCompleted; // does not accept tasks anymore, may still be running

        private Task _runningTask;
        private CancellationTokenSource _tokenSource;

        private bool _terminating; // ensures we raise that event only once
        private bool _terminated; // remember we've terminated
        private TaskCompletionSource<int> _terminatedSource; // awaitable source

        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskError;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskStarting;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskCompleted;
        internal event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskCancelled;
        
        // triggers when the runner stops (but could start again if a task is added to it)
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Stopped;

        // triggers when the hosting environment requests that the runner terminates
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Terminating;

        // triggers when the runner terminates (no task can be added, no task is running)
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Terminated;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        public BackgroundTaskRunner(ILogger logger)
            : this(typeof (T).FullName, new BackgroundTaskRunnerOptions(), logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the runner.</param>
        /// <param name="logger"></param>
        public BackgroundTaskRunner(string name, ILogger logger)
            : this(name, new BackgroundTaskRunnerOptions(), logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="options">The set of options.</param>
        /// <param name="logger"></param>
        public BackgroundTaskRunner(BackgroundTaskRunnerOptions options, ILogger logger)
            : this(typeof (T).FullName, options, logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="name">The name of the runner.</param>
        /// <param name="options">The set of options.</param>
        /// <param name="logger"></param>
        public BackgroundTaskRunner(string name, BackgroundTaskRunnerOptions options, ILogger logger)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (logger == null) throw new ArgumentNullException("logger");
            _options = options;
            _logPrefix = "[" + name + "] ";
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
        /// Gets the running task as an immutable object.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no running task.</exception>
        /// <remarks>
        /// Unless the AutoStart option is true, there will be no running task until
        /// a background task is added to the queue. Unless the KeepAlive option is true, there
        /// will be no running task when the queue is empty.
        /// </remarks>
        public ThreadingTaskImmutable CurrentThreadingTask
        {
            get
            {
                lock (_locker)
                {
                    if (_runningTask == null)
                        throw new InvalidOperationException("There is no current Threading.Task.");
                    return new ThreadingTaskImmutable(_runningTask);
                }
            }
        }

        /// <summary>
        /// Gets an awaitable used to await the runner running operation.
        /// </summary>
        /// <returns>An awaitable instance.</returns>
        /// <remarks>Used to wait until the runner is no longer running (IsRunning == false),
        /// though the runner could be started again afterwards by adding tasks to it.</remarks>
        public ThreadingTaskImmutable StoppedAwaitable
        {
            get
            {
                lock (_locker)
                {
                    var task = _runningTask ?? Task.FromResult(0);
                    return new ThreadingTaskImmutable(task);
                }
            }
        }

        /// <summary>
        /// Gets an awaitable used to await the runner.
        /// </summary>
        /// <returns>An awaitable instance.</returns>
        /// <remarks>Used to wait until the runner is terminated.</remarks>
        public ThreadingTaskImmutable TerminatedAwaitable
        {
            get
            {
                lock (_locker)
                {
                    if (_terminatedSource == null && _terminated == false)
                        _terminatedSource = new TaskCompletionSource<int>();
                    var task = _terminatedSource == null ? Task.FromResult(0) : _terminatedSource.Task;
                    return new ThreadingTaskImmutable(task);
                }
            }
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
                _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Task added {0}", task.GetType);
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
                _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Task added {0}", task.GetType);
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
            _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Starting");
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
                Thread.Sleep(100); // give time to CompleteAdding()
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
                var onStopped = false;
                lock (_locker)
                {
                    if (token.IsCancellationRequested || _tasks.Count == 0)
                    {
                        _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Stopping");

                        if (_options.PreserveRunningTask == false)
                            _runningTask = null;

                        // stopped
                        _isRunning = false;
                        onStopped = true;
                    }
                }

                if (onStopped)
                {
                    OnEvent(Stopped, "Stopped");
                    return;
                }

                // if _runningTask is taskSource.Task then we must keep continuing it,
                // not starting a new taskSource, else _runningTask would complete and
                // something may be waiting on it
                //PumpIBackgroundTasks(factory, token); // restart
                // ReSharper disable MethodSupportsCancellation // always run
                t.ContinueWithTask(_ => PumpIBackgroundTasks(factory, token)); // restart
                // ReSharper restore MethodSupportsCancellation
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
                    _logger.Error<BackgroundTaskRunner>(_logPrefix + "Task runner exception.", exception);
                }

                // is it ok to run?
                if (TaskSourceCanceled(taskSource, token)) return;

                // try to get a task
                // the blocking MoveNext will end if token is cancelled or collection is completed
                T bgTask;
                var hasBgTask = _options.KeepAlive
                    // ReSharper disable once PossibleNullReferenceException
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

        private static bool TaskSourceCanceled(TaskCompletionSource<object> taskSource, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                taskSource.SetCanceled();
                return true;
            }
            return false;
        }

        private static void TaskSourceCompleted(TaskCompletionSource<object> taskSource, CancellationToken token)
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
                _logger.Error<BackgroundTaskRunner>(_logPrefix + "Task has failed", ex);
            }            
        }

        #region Events

        private void OnEvent(TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> handler, string name)
        {
            if (handler == null) return;
            OnEvent(handler, name, EventArgs.Empty);
        }

        private void OnEvent<TArgs>(TypedEventHandler<BackgroundTaskRunner<T>, TArgs> handler, string name, TArgs e)
        {
            if (handler == null) return;

            try
            {
                handler(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error<BackgroundTaskRunner>(_logPrefix + name + " exception occurred", ex);
            }
        }

        protected virtual void OnTaskError(TaskEventArgs<T> e)
        {
            OnEvent(TaskError, "TaskError", e);
        }

        protected virtual void OnTaskStarting(TaskEventArgs<T> e)
        {
            OnEvent(TaskStarting, "TaskStarting", e);
        }

        protected virtual void OnTaskCompleted(TaskEventArgs<T> e)
        {
            OnEvent(TaskCompleted, "TaskCompleted", e);
        }

        protected virtual void OnTaskCancelled(TaskEventArgs<T> e)
        {
            OnEvent(TaskCancelled, "TaskCancelled", e);

            //dispose it
            e.Task.Dispose();
        }

        #endregion

        #region IDisposable

        private readonly object _disposalLocker = new object();
        public bool IsDisposed { get; private set; }

        ~BackgroundTaskRunner()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed || disposing == false)
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
            // the first time the hosting environment requests that the runner terminates,
            // raise the Terminating event - that could be used to prevent any process that
            // would expect the runner to be available from starting.
            var onTerminating = false;
            lock (_locker)
            {
                if (_terminating == false)
                {
                    _terminating = true;
                    _logger.Info<BackgroundTaskRunner>(_logPrefix + "Terminating" + (immediate ? " (immediate)" : ""));
                    onTerminating = true;
                }
            }

            if (onTerminating)
                OnEvent(Terminating, "Terminating");

            if (immediate == false)
            {
                // The Stop method is first called with the immediate parameter set to false. The object can either complete
                // processing, call the UnregisterObject method, and then return or it can return immediately and complete
                // processing asynchronously before calling the UnregisterObject method.

                _logger.Info<BackgroundTaskRunner>(_logPrefix + "Waiting for tasks to complete");
                Shutdown(false, false); // do not accept any more tasks, flush the queue, do not wait

                // raise the completed event only after the running task has completed
                // and there's no more task running

                lock (_locker)
                {
                    if (_runningTask != null)
                        _runningTask.ContinueWith(_ => Terminate(false));
                    else
                        Terminate(false);
                }
            }
            else
            {
                // If the registered object does not complete processing before the application manager's time-out
                // period expires, the Stop method is called again with the immediate parameter set to true. When the
                // immediate parameter is true, the registered object must call the UnregisterObject method before returning;
                // otherwise, its registration will be removed by the application manager.

                _logger.Info<BackgroundTaskRunner>(_logPrefix + "Cancelling tasks");
                Shutdown(true, true); // cancel all tasks, wait for the current one to end
                Terminate(true);
            }
        }

        private void Terminate(bool immediate)
        {
            // signal the environment we have terminated
            // log
            // raise the Terminated event
            // complete the awaitable completion source, if any

            HostingEnvironment.UnregisterObject(this);
            _logger.Info<BackgroundTaskRunner>(_logPrefix + "Tasks " + (immediate ? "cancelled" : "completed") + ", terminated");
            OnEvent(Terminated, "Terminated");

            TaskCompletionSource<int> terminatedSource;
            lock (_locker)
            {
                _terminated = true;
                terminatedSource = _terminatedSource;
            }
            if (terminatedSource != null)
                terminatedSource.SetResult(0);
        }
    }
}
