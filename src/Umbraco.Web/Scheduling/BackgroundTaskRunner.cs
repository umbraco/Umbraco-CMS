using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// This is used to create a background task runner which will stay alive in the background of and complete
    /// any tasks that are queued. It is web aware and will ensure that it is shutdown correctly when the app domain
    /// is shutdown.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BackgroundTaskRunner<T> : IDisposable, IRegisteredObject
        where T : IBackgroundTask
    {
        private readonly bool _dedicatedThread;
        private readonly bool _persistentThread;
        private readonly BlockingCollection<T> _tasks = new BlockingCollection<T>();
        private Task _consumer;

        private volatile bool _isRunning = false;
        private static readonly object Locker = new object();
        private CancellationTokenSource _tokenSource;
        internal event EventHandler<TaskEventArgs<T>> TaskError;
        internal event EventHandler<TaskEventArgs<T>> TaskStarting;
        internal event EventHandler<TaskEventArgs<T>> TaskCompleted;
        internal event EventHandler<TaskEventArgs<T>> TaskCancelled;

        public BackgroundTaskRunner(bool dedicatedThread = false, bool persistentThread = false)
        {
            _dedicatedThread = dedicatedThread;
            _persistentThread = persistentThread;
            HostingEnvironment.RegisterObject(this);
        }

        public int TaskCount
        {
            get { return _tasks.Count; }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public TaskStatus TaskStatus
        {
            get { return _consumer.Status; }
        }

      
        /// <summary>
        /// Returns the task awaiter so that consumers of the BackgroundTaskManager can await
        /// the threading operation.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is just the coolest thing ever, check this article out:
        /// http://blogs.msdn.com/b/pfxteam/archive/2011/01/13/10115642.aspx
        /// 
        /// So long as we have a method called GetAwaiter() that returns an instance of INotifyCompletion 
        /// we can await anything! :)
        /// </remarks>
        public TaskAwaiter GetAwaiter()
        {
            return _consumer.GetAwaiter();
        }

        public void Add(T task)
        {
            //add any tasks first
            LogHelper.Debug<BackgroundTaskRunner<T>>(" Task added {0}", () => task.GetType());
            _tasks.Add(task);

            //ensure's everything is started
            StartUp();
        }

        public void StartUp()
        {
            if (!_isRunning)
            {
                lock (Locker)
                {
                    //double check 
                    if (!_isRunning)
                    {
                        _isRunning = true;
                        //Create a new token source since this is a new proces
                        _tokenSource = new CancellationTokenSource();
                        StartConsumer();
                        LogHelper.Debug<BackgroundTaskRunner<T>>("Starting");
                    }
                }
            }
        }

        public void ShutDown()
        {
            lock (Locker)
            {
                _isRunning = false;

                try
                {
                    if (_consumer != null)
                    {
                        //cancel all operations
                        _tokenSource.Cancel();

                        try
                        {
                            _consumer.Wait();
                        }
                        catch (AggregateException e)
                        {
                            //NOTE: We are logging Debug because we are expecting these errors

                            LogHelper.Debug<BackgroundTaskRunner<T>>("AggregateException thrown with the following inner exceptions:");
                            // Display information about each exception.  
                            foreach (var v in e.InnerExceptions)
                            {
                                var exception = v as TaskCanceledException;
                                if (exception != null)
                                {
                                    LogHelper.Debug<BackgroundTaskRunner<T>>("   .Net TaskCanceledException: .Net Task ID {0}", () => exception.Task.Id);
                                }
                                else
                                {
                                    LogHelper.Debug<BackgroundTaskRunner<T>>("   Exception: {0}", () => v.GetType().Name);
                                }
                            }
                        }
                    }

                    if (_tasks.Count > 0)
                    {
                        LogHelper.Debug<BackgroundTaskRunner<T>>("Processing remaining tasks before shutdown: {0}", () => _tasks.Count);

                        //now we need to ensure the remaining queue is processed if there's any remaining,
                        // this will all be processed on the current/main thread.
                        T remainingTask;
                        while (_tasks.TryTake(out remainingTask))
                        {
                            ConsumeTaskInternal(remainingTask);
                        }
                    }

                    LogHelper.Debug<BackgroundTaskRunner<T>>("Shutdown");

                    //disposing these is really optional since they'll be disposed immediately since they are no longer running
                    //but we'll put this here anyways.
                    if (_consumer != null && (_consumer.IsCompleted || _consumer.IsCanceled))
                    {
                        _consumer.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<BackgroundTaskRunner<T>>("Error occurred shutting down task runner", ex);
                }
                finally
                {
                    HostingEnvironment.UnregisterObject(this);
                }
            }
        }

        /// <summary>
        /// Starts the consumer task
        /// </summary>
        private void StartConsumer()
        {
            var token = _tokenSource.Token;

            _consumer = Task.Factory.StartNew(() =>
                StartThread(token),
                token,
                _dedicatedThread ? TaskCreationOptions.LongRunning : TaskCreationOptions.None,
                TaskScheduler.Default);

            //if this is not a persistent thread, wait till it's done and shut ourselves down
            // thus ending the thread or giving back to the thread pool. If another task is added
            // another thread will spawn or be taken from the pool to process.
            if (!_persistentThread)
            {
                _consumer.ContinueWith(task => ShutDown());
            }

        }

        /// <summary>
        /// Invokes a new worker thread to consume tasks
        /// </summary>
        /// <param name="token"></param>
        private void StartThread(CancellationToken token)
        {
            // Was cancellation already requested?  
            if (token.IsCancellationRequested)
            {
                LogHelper.Info<BackgroundTaskRunner<T>>("Thread {0} was cancelled before it got started.", () => Thread.CurrentThread.ManagedThreadId);
                token.ThrowIfCancellationRequested();
            }

            TakeAndConsumeTask(token);
        }

        /// <summary>
        /// Trys to get a task from the queue, if there isn't one it will wait a second and try again
        /// </summary>
        /// <param name="token"></param>
        private void TakeAndConsumeTask(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                LogHelper.Info<BackgroundTaskRunner<T>>("Thread {0} was cancelled.", () => Thread.CurrentThread.ManagedThreadId);
                token.ThrowIfCancellationRequested();
            }

            //If this is true, the thread will stay alive and just wait until there is anything in the queue
            // and process it. When there is nothing in the queue, the thread will just block until there is
            // something to process.
            //When this is false, the thread will process what is currently in the queue and once that is 
            // done, the thread will end and we will shutdown the process

            if (_persistentThread)
            {
                //This will iterate over the collection, if there is nothing to take
                // the thread will block until there is something available.
                //We need to pass our cancellation token so that the thread will
                // cancel when we shutdown
                foreach (var t in _tasks.GetConsumingEnumerable(token))
                {
                    ConsumeTaskCancellable(t, token);
                }

                //recurse and keep going
                TakeAndConsumeTask(token);
            }
            else
            {
                T repositoryTask;
                while (_tasks.TryTake(out repositoryTask))
                {
                    ConsumeTaskCancellable(repositoryTask, token);
                }

                //the task will end here
            }
        }

        internal void ConsumeTaskCancellable(T task, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                OnTaskCancelled(new TaskEventArgs<T>(task));

                //NOTE: Since the task hasn't started this is pretty pointless so leaving it out.
                LogHelper.Info<BackgroundTaskRunner<T>>("Task {0}) was cancelled.",
                    () => task.GetType());

                token.ThrowIfCancellationRequested();
            }

            ConsumeTaskInternal(task);
        }

        private void ConsumeTaskInternal(T task)
        {
            try
            {
                OnTaskStarting(new TaskEventArgs<T>(task));

                try
                {
                    using (task)
                    {
                        task.Run();
                    }
                }
                catch (Exception e)
                {
                    OnTaskError(new TaskEventArgs<T>(task, e));
                    throw;
                }

                OnTaskCompleted(new TaskEventArgs<T>(task));
            }
            catch (Exception ex)
            {
                LogHelper.Error<BackgroundTaskRunner<T>>("An error occurred consuming task", ex);
            }
        }

        protected virtual void OnTaskError(TaskEventArgs<T> e)
        {
            var handler = TaskError;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnTaskStarting(TaskEventArgs<T> e)
        {
            var handler = TaskStarting;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnTaskCompleted(TaskEventArgs<T> e)
        {
            var handler = TaskCompleted;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnTaskCancelled(TaskEventArgs<T> e)
        {
            var handler = TaskCancelled;
            if (handler != null) handler(this, e);
        }


        #region Disposal
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
            if (this.IsDisposed || !disposing)
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
            ShutDown();
        }
        #endregion

        public void Stop(bool immediate)
        {
            if (immediate == false)
            {
                LogHelper.Debug<BackgroundTaskRunner<T>>("Application is shutting down, waiting for tasks to complete");
                Dispose();
            }
            else
            {
                //NOTE: this will thread block the current operation if the manager
                // is still shutting down because the Shutdown operation is also locked
                // by this same lock instance. This would only matter if Stop is called by ASP.Net
                // on two different threads though, otherwise the current thread will just block normally
                // until the app is shutdown
                lock (Locker)
                {
                    LogHelper.Info<BackgroundTaskRunner<T>>("Application is shutting down immediately");
                }
            }

        }

    }
}
