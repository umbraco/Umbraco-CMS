using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Manages a queue of tasks and runs them in the background.
    /// </summary>
    /// <remarks>This class exists for logging purposes - the one you want to use is BackgroundTaskRunner{T}.</remarks>
    public abstract class BackgroundTaskRunner
    { }

    /// <summary>
    /// Manages a queue of tasks of type <typeparamref name="T"/> and runs them in the background.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    /// <remarks>The task runner is web-aware and will ensure that it shuts down correctly when the AppDomain
    /// shuts down (ie is unloaded).</remarks>
    public class BackgroundTaskRunner<T> : BackgroundTaskRunner, IBackgroundTaskRunner<T>
        where T : class, IBackgroundTask
    {
        private readonly string _logPrefix;
        private readonly BackgroundTaskRunnerOptions _options;
        private readonly ILogger _logger;
        private readonly object _locker = new object();

        private readonly BlockingCollection<T> _tasks = new BlockingCollection<T>();
        private IEnumerator<T> _enumerator;

        // that event is used to stop the pump when it is alive and waiting
        // on a latched task - so it waits on the latch, the cancellation token,
        // and the completed event
        private readonly ManualResetEventSlim _completedEvent = new ManualResetEventSlim(false);

        // in various places we are testing these vars outside a lock, so make them volatile
        private volatile bool _isRunning; // is running
        private volatile bool _completed; // does not accept tasks anymore, may still be running

        private Task _runningTask; // the threading task that is currently executing background tasks
        private CancellationTokenSource _shutdownTokenSource; // used to cancel everything and shutdown
        private CancellationTokenSource _cancelTokenSource; // used to cancel the current task
        private CancellationToken _shutdownToken;

        private bool _terminating; // ensures we raise that event only once
        private bool _terminated; // remember we've terminated
        private TaskCompletionSource<int> _terminatedSource; // enable awaiting termination

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="mainDomInstall">An optional action to execute when the main domain status is aquired.</param>
        /// <param name="mainDomRelease">An optional action to execute when the main domain status is released.</param>
        public BackgroundTaskRunner(ILogger logger, Action mainDomInstall = null, Action mainDomRelease = null)
            : this(typeof (T).FullName, new BackgroundTaskRunnerOptions(), logger, mainDomInstall, mainDomRelease)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the runner.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="mainDomInstall">An optional action to execute when the main domain status is aquired.</param>
        /// <param name="mainDomRelease">An optional action to execute when the main domain status is released.</param>
        public BackgroundTaskRunner(string name, ILogger logger, Action mainDomInstall = null, Action mainDomRelease = null)
            : this(name, new BackgroundTaskRunnerOptions(), logger, mainDomInstall, mainDomRelease)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="options">The set of options.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="mainDomInstall">An optional action to execute when the main domain status is aquired.</param>
        /// <param name="mainDomRelease">An optional action to execute when the main domain status is released.</param>
        public BackgroundTaskRunner(BackgroundTaskRunnerOptions options, ILogger logger, Action mainDomInstall = null, Action mainDomRelease = null)
            : this(typeof (T).FullName, options, logger, mainDomInstall, mainDomRelease)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="name">The name of the runner.</param>
        /// <param name="options">The set of options.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="mainDomInstall">An optional action to execute when the main domain status is aquired.</param>
        /// <param name="mainDomRelease">An optional action to execute when the main domain status is released.</param>
        public BackgroundTaskRunner(string name, BackgroundTaskRunnerOptions options, ILogger logger, Action mainDomInstall = null, Action mainDomRelease = null)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (logger == null) throw new ArgumentNullException("logger");
            _options = options;
            _logPrefix = "[" + name + "] ";
            _logger = logger;

            if (options.Hosted)
                HostingEnvironment.RegisterObject(this);

            if (mainDomInstall != null || mainDomRelease != null)
            {
                var appContext = ApplicationContext.Current;
                var mainDom = appContext == null ? null : appContext.MainDom;
                var reg = mainDom == null || ApplicationContext.Current.MainDom.Register(mainDomInstall, mainDomRelease);
                if (reg == false)
                    _completed = _terminated = true;
                if (reg && mainDom == null && mainDomInstall != null)
                    mainDomInstall();
            }

            if (options.AutoStart && _terminated == false)
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
        /// Gets a value indicating whether a threading task is currently running.
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
            get { return _completed; }
        }

        /// <summary>
        /// Gets the running threading task as an immutable awaitable.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no running task.</exception>
        /// <remarks>
        /// <para>Unless the AutoStart option is true, there will be no current threading task until
        /// a background task is added to the queue, and there will be no current threading task
        /// when the queue is empty. In which case this method returns null.</para>
        /// <para>The returned value can be awaited and that is all (eg no continuation).</para>
        /// </remarks>
        internal ThreadingTaskImmutable CurrentThreadingTask
        {
            get
            {
                lock (_locker)
                {
                    return _runningTask == null ? null : new ThreadingTaskImmutable(_runningTask);
                }
            }
        }

        // fixme should the above throw, return null, a completed task?

        // fixme what's the diff?!
        /// <summary>
        /// Gets an awaitable used to await the runner running operation.
        /// </summary>
        /// <returns>An awaitable instance.</returns>
        /// <remarks>Used to wait until the runner is no longer running (IsRunning == false),
        /// though the runner could be started again afterwards by adding tasks to it.</remarks>
        internal ThreadingTaskImmutable StoppedAwaitable
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
        /// Gets an awaitable object that can be used to await for the runner to terminate.
        /// </summary>
        /// <returns>An awaitable object.</returns>
        /// <remarks>
        /// <para>Used to wait until the runner has terminated.</para>
        /// <para>This is for unit tests and should not be used otherwise. In most cases when the runner
        /// has terminated, the application domain is going down and it is not the right time to do things.</para>
        /// </remarks>
        internal ThreadingTaskImmutable TerminatedAwaitable
        {
            get
            {
                lock (_locker)
                {
                    if (_terminatedSource == null)
                        _terminatedSource = new TaskCompletionSource<int>();
                    if (_terminated)
                        _terminatedSource.SetResult(0);
                    return new ThreadingTaskImmutable(_terminatedSource.Task);
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
                if (_completed)
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
                if (_completed)
                {
                    _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Task cannot be added {0}, the task runner has already shutdown", task.GetType);
                    return false;
                }

                // add task
                _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Task added {0}", task.GetType);
                _tasks.Add(task);

                // start
                StartUpLocked();

                return true;
            }
        }

        /// <summary>
        /// Cancels to current task, if any.
        /// </summary>
        /// <remarks>Has no effect if the task runs synchronously, or does not want to cancel.</remarks>
        public void CancelCurrentBackgroundTask()
        {
            lock (_locker)
            {
                if (_completed)
                    throw new InvalidOperationException("The task runner has completed.");
                if (_cancelTokenSource != null)
                    _cancelTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Starts the tasks runner, if not already running.
        /// </summary>
        /// <remarks>Is invoked each time a task is added, to ensure it is going to be processed.</remarks>
        /// <exception cref="InvalidOperationException">The task runner has completed.</exception>
        internal void StartUp()
        {
            if (_isRunning) return;

            lock (_locker)
            {
                if (_completed)
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
            _shutdownTokenSource = new CancellationTokenSource();
            _shutdownToken = _shutdownTokenSource.Token;

            _enumerator = _options.KeepAlive ? _tasks.GetConsumingEnumerable(_shutdownToken).GetEnumerator() : null;
            _runningTask = Task.Run(async () => await Pump(), _shutdownToken);

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
                _completed = true; // do not accept new tasks
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
                _shutdownTokenSource.Cancel(false); // false is the default
            }

            // tasks in the queue will be executed...
            if (wait == false) return;

            if (_runningTask != null)
                _runningTask.Wait(); // wait for whatever is running to end...
        }

        private async Task Pump()
        {
            while (true)
            {
                var bgTask = GetNextBackgroundTask();
                if (bgTask == null)
                    return;

                lock (_locker)
                {
                    // set another one - for the next task
                    _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownToken);
                }

                bgTask = WaitForLatch(bgTask, _cancelTokenSource.Token);
                if (bgTask == null) return;

                try
                {
                    await RunAsync(bgTask, _cancelTokenSource.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // RunAsync should NOT throw exception - just raise an event
                    // this is here for safety and to ensure we don't kill everything, ever
                    _logger.Error<BackgroundTaskRunner>(_logPrefix + "Task runner exception.", e);
                }

                _cancelTokenSource = null;
            }
        }

        private T GetNextBackgroundTask()
        {
            while (true)
            {
                // exit if cancelling
                if (_shutdownToken.IsCancellationRequested == false)
                {
                    // try to get a task
                    // the blocking MoveNext will end if token is cancelled or collection is completed
                    T bgTask;
                    var hasBgTask = _options.KeepAlive
                        ? (bgTask = _enumerator.MoveNext() ? _enumerator.Current : null) != null // blocking
                        : _tasks.TryTake(out bgTask); // non-blocking

                    // exit if cancelling
                    if (_shutdownToken.IsCancellationRequested == false && hasBgTask)
                        return bgTask;
                }

                lock (_locker)
                {
                    if (_shutdownToken.IsCancellationRequested == false && _tasks.Count > 0) continue;

                    _logger.Debug<BackgroundTaskRunner>(_logPrefix + "Stopping");

                    if (_options.PreserveRunningTask == false)
                        _runningTask = null;

                    _isRunning = false;

                    _shutdownToken = CancellationToken.None;
                    _enumerator = null;
                }

                OnEvent(Stopped, "Stopped");
                return null;
            }
        }

        private T WaitForLatch(T bgTask, CancellationToken token)
        {
            var latched = bgTask as ILatchedBackgroundTask;
            if (latched == null || latched.IsLatched == false) return bgTask;

            // returns the array index of the object that satisfied the wait
            var i = WaitHandle.WaitAny(new[] { latched.Latch, token.WaitHandle, _completedEvent.WaitHandle });

            switch (i)
            {
                case 0:
                    // ok to run now
                    return bgTask;
                case 1:
                    // cancellation
                    return null;
                case 2:
                    // termination
                    if (latched.RunsOnShutdown) return bgTask;
                    latched.Dispose();
                    return null;
                default:
                    throw new Exception("panic.");
            }
        }

        private async Task RunAsync(T bgTask, CancellationToken token)
        {
            try
            {
                OnTaskStarting(new TaskEventArgs<T>(bgTask));

                try
                {
                    try
                    {
                        if (bgTask.IsAsync)
                            //configure await = false since we don't care about the context, we're on a background thread.
                            await bgTask.RunAsync(token).ConfigureAwait(false);
                        else
                            bgTask.Run();
                    }
                    finally // ensure we disposed - unless latched again ie wants to re-run
                    {
                        var lbgTask = bgTask as ILatchedBackgroundTask;
                        if (lbgTask == null || lbgTask.IsLatched == false)
                            bgTask.Dispose();
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

        // triggers when a background task starts
        public event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskStarting;

        // triggers when a background task has completed
        public event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskCompleted;

        // triggers when a background task throws
        public event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskError;

        // triggers when a background task is cancelled
        public event TypedEventHandler<BackgroundTaskRunner<T>, TaskEventArgs<T>> TaskCancelled;

        // triggers when the runner stops (but could start again if a task is added to it)
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Stopped;

        // triggers when the hosting environment requests that the runner terminates
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Terminating;

        // triggers when the runner has terminated (no task can be added, no task is running)
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Terminated;

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

                // raise the completed event only after the running threading task has completed
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

        // called by Stop either immediately or eventually
        private void Terminate(bool immediate)
        {
            // signal the environment we have terminated
            // log
            // raise the Terminated event
            // complete the awaitable completion source, if any

            HostingEnvironment.UnregisterObject(this);

            TaskCompletionSource<int> terminatedSource;
            lock (_locker)
            {
                _terminated = true;
                terminatedSource = _terminatedSource;
            }

            _logger.Info<BackgroundTaskRunner>(_logPrefix + "Tasks " + (immediate ? "cancelled" : "completed") + ", terminated");

            OnEvent(Terminated, "Terminated");

            if (terminatedSource != null)
                terminatedSource.SetResult(0);
        }
    }
}
