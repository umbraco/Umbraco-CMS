using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
    {
        /// <summary>
        /// Represents a MainDom hook.
        /// </summary>
        public class MainDomHook
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MainDomHook"/> class.
            /// </summary>
            /// <param name="mainDom">The <see cref="IMainDom"/> object.</param>
            /// <param name="install">A method to execute when hooking into the main domain.</param>
            /// <param name="release">A method to execute when the main domain releases.</param>
            public MainDomHook(IMainDom mainDom, Action install, Action release)
            {
                MainDom = mainDom;
                Install = install;
                Release = release;
            }

            /// <summary>
            /// Gets the <see cref="IMainDom"/> object.
            /// </summary>
            public IMainDom MainDom { get; }

            /// <summary>
            /// Gets the method to execute when hooking into the main domain.
            /// </summary>
            public Action Install { get; }

            /// <summary>
            /// Gets the method to execute when the main domain releases.
            /// </summary>
            public Action Release { get; }

            internal bool Register()
            {
                if (MainDom != null)
                    return MainDom.Register(Install, Release);

                // tests
                Install?.Invoke();
                return true;
            }
        }
    }

    /// <summary>
    /// Manages a queue of tasks of type <typeparamref name="T"/> and runs them in the background.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    /// <remarks>The task runner is web-aware and will ensure that it shuts down correctly when the AppDomain
    /// shuts down (ie is unloaded).</remarks>
    public class BackgroundTaskRunner<T> : BackgroundTaskRunner, IBackgroundTaskRunner<T>
        where T : class, IBackgroundTask
    {
        // do not remove this comment!
        //
        // if you plan to do anything on this class, first go and read
        // http://blog.stephencleary.com/2012/12/dont-block-in-asynchronous-code.html
        // http://stackoverflow.com/questions/19481964/calling-taskcompletionsource-setresult-in-a-non-blocking-manner
        // http://stackoverflow.com/questions/21225361/is-there-anything-like-asynchronous-blockingcollectiont
        // and more, and more, and more
        // and remember: async is hard

        private readonly string _logPrefix;
        private readonly BackgroundTaskRunnerOptions _options;
        private readonly ILogger _logger;
        private readonly object _locker = new object();

        private readonly BufferBlock<T> _tasks = new BufferBlock<T>(new DataflowBlockOptions());

        // in various places we are testing these vars outside a lock, so make them volatile
        private volatile bool _isRunning; // is running
        private volatile bool _completed; // does not accept tasks anymore, may still be running

        private Task _runningTask; // the threading task that is currently executing background tasks
        private CancellationTokenSource _shutdownTokenSource; // used to cancel everything and shutdown
        private CancellationTokenSource _cancelTokenSource; // used to cancel the current task
        private CancellationToken _shutdownToken;

        private bool _terminating; // ensures we raise that event only once
        private bool _terminated; // remember we've terminated
        private readonly TaskCompletionSource<int> _terminatedSource = new TaskCompletionSource<int>(); // enable awaiting termination

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="hook">An optional main domain hook.</param>
        public BackgroundTaskRunner(ILogger logger, MainDomHook hook = null)
            : this(typeof(T).FullName, new BackgroundTaskRunnerOptions(), logger, hook)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the runner.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="hook">An optional main domain hook.</param>
        public BackgroundTaskRunner(string name, ILogger logger, MainDomHook hook = null)
            : this(name, new BackgroundTaskRunnerOptions(), logger, hook)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="options">The set of options.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="hook">An optional main domain hook.</param>
        public BackgroundTaskRunner(BackgroundTaskRunnerOptions options, ILogger logger, MainDomHook hook = null)
            : this(typeof(T).FullName, options, logger, hook)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunner{T}"/> class with a set of options.
        /// </summary>
        /// <param name="name">The name of the runner.</param>
        /// <param name="options">The set of options.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="hook">An optional main domain hook.</param>
        public BackgroundTaskRunner(string name, BackgroundTaskRunnerOptions options, ILogger logger, MainDomHook hook = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logPrefix = "[" + name + "] ";

            if (options.Hosted)
                HostingEnvironment.RegisterObject(this);

            if (hook != null)
                _completed = _terminated = hook.Register() == false;

            if (options.AutoStart && _terminated == false)
                StartUp();
        }

        /// <summary>
        /// Gets the number of tasks in the queue.
        /// </summary>
        public int TaskCount => _tasks.Count;

        /// <summary>
        /// Gets a value indicating whether a threading task is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets a value indicating whether the runner has completed and cannot accept tasks anymore.
        /// </summary>
        public bool IsCompleted => _completed;

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

        /// <summary>
        /// Gets an awaitable used to await the runner running operation.
        /// </summary>
        /// <returns>An awaitable instance.</returns>
        /// <remarks>Used to wait until the runner is no longer running (IsRunning == false),
        /// though the runner could be started again afterwards by adding tasks to it. If
        /// the runner is not running, returns a completed awaitable.</remarks>
        public ThreadingTaskImmutable StoppedAwaitable
        {
            get
            {
                lock (_locker)
                {
                    var task = _runningTask ?? Task.CompletedTask;
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
        /// <para>
        /// The only time the runner will be terminated is by the Hosting Environment when the application is being shutdown. 
        /// </para>
        /// </remarks>
        internal ThreadingTaskImmutable TerminatedAwaitable
        {
            get
            {
                lock (_locker)
                {
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
                _logger.Debug<BackgroundTaskRunner, string, string>("{LogPrefix} Task Added {TaskType}", _logPrefix , task.GetType().FullName);
                _tasks.Post(task);

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
                    _logger.Debug<BackgroundTaskRunner, string, string>("{LogPrefix} Task cannot be added {TaskType}, the task runner has already shutdown", _logPrefix, task.GetType().FullName);
                    return false;
                }

                // add task
                _logger.Debug<BackgroundTaskRunner, string, string>("{LogPrefix} Task added {TaskType}", _logPrefix, task.GetType().FullName);
                _tasks.Post(task);

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
                _cancelTokenSource?.Cancel();
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
            using (ExecutionContext.SuppressFlow())
            {
                _runningTask = Task.Run(async () => await Pump().ConfigureAwait(false), _shutdownToken);
            }   

            _logger.Debug<BackgroundTaskRunner, string>("{LogPrefix} Starting", _logPrefix);
        }

        /// <summary>
        /// Shuts the tasks runner down.
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

            var hasTasks = TaskCount > 0;

            if (!force && hasTasks)
                _logger.Info<BackgroundTaskRunner, string>("{LogPrefix} Waiting for tasks to complete", _logPrefix);

            // complete the queue
            // will stop waiting on the queue or on a latch
            _tasks.Complete();

            if (force)
            {
                // we must bring everything down, now                
                lock (_locker)
                {
                    // was Complete() enough?
                    // if _tasks.Complete() ended up triggering code to stop the runner and reset
                    // the _isRunning flag, then there's no need to initiate a cancel on the cancelation token.
                    if (_isRunning == false)
                        return; 
                }

                // try to cancel running async tasks (cannot do much about sync tasks)
                // break latched tasks
                // stop processing the queue
                _shutdownTokenSource?.Cancel(false); // false is the default
                _shutdownTokenSource?.Dispose();
                _shutdownTokenSource = null;
            }

            // tasks in the queue will be executed...
            if (!wait) return;

            _runningTask?.Wait(CancellationToken.None); // wait for whatever is running to end...
        }

        private async Task Pump()
        {
            while (true)
            {
                // get the next task
                // if it returns null the runner is going down, stop
                var bgTask = await GetNextBackgroundTask(_shutdownToken);
                if (bgTask == null) return;

                // set a cancellation source so that the current task can be cancelled
                // link from _shutdownToken so that we can use _cancelTokenSource for both
                lock (_locker)
                {
                    _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownToken);
                }

                try
                {
                    // wait for latch should return the task
                    // if it returns null it's either that the task has been cancelled
                    // or the whole runner is going down - in both cases, continue,
                    // and GetNextBackgroundTask will take care of shutdowns
                    bgTask = await WaitForLatch(bgTask, _cancelTokenSource.Token);

                    if (bgTask != null)
                    {
                        // executes & be safe - RunAsync should NOT throw but only raise an event,
                        // but... just make sure we never ever take everything down
                        try
                        {
                            await RunAsync(bgTask, _cancelTokenSource.Token).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<BackgroundTaskRunner, string>(ex, "{LogPrefix} Task runner exception", _logPrefix);
                        }
                    }
                }
                finally
                {
                    // done
                    lock (_locker)
                    {
                        // always dispose CancellationTokenSource when you are done using them
                        // https://lowleveldesign.org/2015/11/30/catch-in-cancellationtokensource/
                        _cancelTokenSource.Dispose();
                        _cancelTokenSource = null;
                    }
                }
            }
        }

        // gets the next background task from the buffer
        private async Task<T> GetNextBackgroundTask(CancellationToken token)
        {
            while (true)
            {
                var task = await GetNextBackgroundTask2(token);
                if (task != null) return task;

                lock (_locker)
                {
                    // deal with race condition
                    if (_shutdownToken.IsCancellationRequested == false && TaskCount > 0) continue;

                    // if we really have nothing to do, stop
                    _logger.Debug<BackgroundTaskRunner, string>("{LogPrefix} Stopping", _logPrefix);

                    if (_options.PreserveRunningTask == false)
                        _runningTask = null;
                    _isRunning = false;
                    _shutdownToken = CancellationToken.None;
                }

                OnEvent(Stopped, "Stopped");
                return null;
            }
        }

        private async Task<T> GetNextBackgroundTask2(CancellationToken shutdownToken)
        {
            // exit if canceling
            if (shutdownToken.IsCancellationRequested)
                return null;

            // if KeepAlive is false then don't block, exit if there is
            // no task in the buffer - yes, there is a race condition, which
            // we'll take care of
            if (_options.KeepAlive == false && TaskCount == 0)
                return null;

            try
            {
                // A Task<TResult> that informs of whether and when more output is available. If, when the
                // task completes, its Result is true, more output is available in the source (though another
                // consumer of the source may retrieve the data). If it returns false, more output is not
                // and will never be available, due to the source completing prior to output being available.

                var output = await _tasks.OutputAvailableAsync(shutdownToken); // block until output or cancelled
                if (output == false) return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }

            try
            {
                // A task that represents the asynchronous receive operation. When an item value is successfully
                // received from the source, the returned task is completed and its Result returns the received
                // value. If an item value cannot be retrieved because the source is empty and completed, an
                // InvalidOperationException exception is thrown in the returned task.

                // the source cannot be empty *and* completed here - we know we have output
                return await _tasks.ReceiveAsync(shutdownToken);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        // if bgTask is not a latched background task, or if it is not latched, returns immediately
        // else waits for the latch, taking care of completion and shutdown and whatnot
        private async Task<T> WaitForLatch(T bgTask, CancellationToken token)
        {
            var latched = bgTask as ILatchedBackgroundTask;
            if (latched == null || latched.IsLatched == false) return bgTask;

            // support canceling awaiting
            // read https://github.com/dotnet/corefx/issues/2704
            // read http://stackoverflow.com/questions/27238232/how-can-i-cancel-task-whenall
            var tokenTaskSource = new TaskCompletionSource<bool>();
            token.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tokenTaskSource);

            // returns the task that completed
            // - latched.Latch completes when the latch releases
            // - _tasks.Completion completes when the runner completes
            // -  tokenTaskSource.Task completes when this task, or the whole runner is cancelled
            var task = await Task.WhenAny(latched.Latch, _tasks.Completion, tokenTaskSource.Task);

            // ok to run now
            if (task == latched.Latch)
                return bgTask;

            // we are shutting down if the _tasks.Complete(); was called or the shutdown token was cancelled
            var isShuttingDown = _shutdownToken.IsCancellationRequested || task == _tasks.Completion;

            // if shutting down, return the task only if it runs on shutdown
            if (isShuttingDown && latched.RunsOnShutdown)
                return bgTask;

            // else, either it does not run on shutdown or it's been cancelled, dispose
            latched.Dispose();
            return null;
        }

        // runs the background task, taking care of shutdown (as far as possible - cannot abort
        // a non-async Run for example, so we'll do our best)
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
                        {
                            // configure await = false since we don't care about the context, we're on a background thread.
                            await bgTask.RunAsync(token).ConfigureAwait(false);
                        }
                        else
                        {
                            bgTask.Run();
                        }
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

                _logger.Error<BackgroundTaskRunner, string>(ex, "{LogPrefix} Task has failed", _logPrefix);
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

        // triggers when the hosting environment has terminated (no task can be added, no task is running)
        internal event TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> Terminated;

        private void OnEvent(TypedEventHandler<BackgroundTaskRunner<T>, EventArgs> handler, string name)
        {
            OnEvent(handler, name, EventArgs.Empty);
        }

        private void OnEvent<TArgs>(TypedEventHandler<BackgroundTaskRunner<T>, TArgs> handler, string name, TArgs e)
        {
            _logger.Debug<BackgroundTaskRunner, string, string>("{LogPrefix} OnEvent {EventName}", _logPrefix, name);

            if (handler == null) return;

            try
            {
                handler(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error<BackgroundTaskRunner, string, string>(ex, "{LogPrefix} {Name} exception occurred", _logPrefix, name);
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

            // dispose it
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

        #region IRegisteredObject.Stop

        /// <summary>
        /// Used by IRegisteredObject.Stop and shutdown on threadpool threads to not block shutdown times.
        /// </summary>
        /// <param name="immediate"></param>
        /// <returns>
        /// An awaitable Task that is used to handle the shutdown.
        /// </returns>
        internal Task StopInternal(bool immediate)
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
                    _logger.Info<BackgroundTaskRunner, string, string>("{LogPrefix} Terminating {Immediate}", _logPrefix, immediate ? immediate.ToString() : string.Empty);
                    onTerminating = true;
                }
            }

            if (onTerminating)
                OnEvent(Terminating, "Terminating");

            // Run the Stop commands on another thread since IRegisteredObject.Stop calls are called sequentially
            // with a single aspnet thread during shutdown and we don't want to delay other calls to IRegisteredObject.Stop.
            if (!immediate)
            {
                using (ExecutionContext.SuppressFlow())
                {
                    return Task.Run(StopInitial, CancellationToken.None);
                }   
            }
            else
            {
                lock (_locker)
                {
                    if (_terminated) return Task.CompletedTask;
                    using (ExecutionContext.SuppressFlow())
                    {
                        return Task.Run(StopImmediate, CancellationToken.None);
                    }
                }
            }
        }

        /// <summary>
        /// Requests a registered object to un-register.
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should un-register from the hosting
        /// environment before returning; otherwise, false.</param>
        /// <remarks>
        /// <para>"When the application manager needs to stop a registered object, it will call the Stop method."</para>
        /// <para>The application manager will call the Stop method to ask a registered object to un-register. During
        /// processing of the Stop method, the registered object must call the HostingEnvironment.UnregisterObject method.</para>
        /// </remarks>
        public void Stop(bool immediate) => StopInternal(immediate);

        /// <summary>
        /// Called when immediate == false for IRegisteredObject.Stop(bool immediate)
        /// </summary>
        /// <remarks>
        /// Called on a threadpool thread
        /// </remarks>
        private void StopInitial()
        {
            // immediate == false when the app is trying to wind down, immediate == true will be called either:
            // after a call with immediate == false or if the app is not trying to wind down and needs to immediately stop.
            // So Stop may be called twice or sometimes only once.

            try
            {
                Shutdown(false, false); // do not accept any more tasks, flush the queue, do not wait
            }
            finally
            {
                // raise the completed event only after the running threading task has completed
                lock (_locker)
                {
                    if (_runningTask != null)
                    {
                        _runningTask.ContinueWith(
                            _ => StopImmediate(),
                            // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                            TaskScheduler.Default);
                    }   
                    else
                    {
                        StopImmediate();
                    }
                        
                }
            }

            // If the shutdown token was not canceled in the Shutdown call above, it means there was still tasks
            // being processed, in which case we'll give it a couple seconds
            if (!_shutdownToken.IsCancellationRequested)
            {
                // If we are called with immediate == false, wind down above and then shutdown within 2 seconds,
                // we want to shut down the app as quick as possible, if we wait until immediate == true, this can
                // take a very long time since immediate will only be true when a new request is received on the new
                // appdomain (or another iis timeout occurs ... which can take some time).
                Thread.Sleep(2000); //we are already on a threadpool thread
                StopImmediate();
            }
        }

        /// <summary>
        /// Called when immediate == true for IRegisteredObject.Stop(bool immediate)
        /// </summary>
        /// <remarks>
        /// Called on a threadpool thread
        /// </remarks>
        private void StopImmediate()
        {
            _logger.Info<BackgroundTaskRunner, string>("{LogPrefix} Canceling tasks", _logPrefix);
            try
            {
                Shutdown(true, true); // cancel all tasks, wait for the current one to end
            }
            finally
            {
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

            if (immediate)
            {
                //only unregister when it's the final call, else we won't be notified of the final call
                HostingEnvironment.UnregisterObject(this);
            }

            if (_terminated) return; // already taken care of

            TaskCompletionSource<int> terminatedSource;
            lock (_locker)
            {
                _terminated = true;
                terminatedSource = _terminatedSource;
            }

            _logger.Info<BackgroundTaskRunner, string, string>("{LogPrefix} Tasks {TaskStatus}, terminated",
                _logPrefix,
                immediate ? "cancelled" : "completed");

            OnEvent(Terminated, "Terminated");

            terminatedSource.TrySetResult(0);
        }

        #endregion
    }
}
