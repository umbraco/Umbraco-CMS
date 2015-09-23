using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using log4net.Core;
using log4net.Util;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// An asynchronous appender based on <see cref="BlockingCollection{T}"/>
    /// </summary>
    /// <remarks>
    /// Based on https://github.com/cjbhaines/Log4Net.Async
    /// </remarks>
    public class ParallelForwardingAppender : AsyncForwardingAppenderBase, IDisposable
    {
        #region Private Members

        private const int DefaultBufferSize = 1000;
        private BlockingCollection<LoggingEventContext> _loggingEvents;
        private CancellationTokenSource _loggingCancelationTokenSource;
        private CancellationToken _loggingCancelationToken;
        private Task _loggingTask;
        private Double _shutdownFlushTimeout = 1;
        private TimeSpan _shutdownFlushTimespan = TimeSpan.FromSeconds(1);
        private static readonly Type ThisType = typeof(ParallelForwardingAppender);
        private volatile bool _shutDownRequested;
        private int? _bufferSize = DefaultBufferSize;

        #endregion Private Members

        #region Properties

        /// <summary>
        /// Gets or sets the number of LoggingEvents that will be buffered.  Set to null for unlimited.
        /// </summary>
        public override int? BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        public int BufferEntryCount
        {
            get
            {
                if (_loggingEvents == null) return 0;
                return _loggingEvents.Count;
            }
        }

        /// <summary>
        /// Gets or sets the time period in which the system will wait for appenders to flush before canceling the background task.
        /// </summary>
        public Double ShutdownFlushTimeout
        {
            get
            {
                return _shutdownFlushTimeout;
            }
            set
            {
                _shutdownFlushTimeout = value;
            }
        }

        protected override string InternalLoggerName
        {
            get { return "ParallelForwardingAppender"; }
        }

        #endregion Properties

        #region Startup

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            _shutdownFlushTimespan = TimeSpan.FromSeconds(_shutdownFlushTimeout);
            StartForwarding();
        }

        private void StartForwarding()
        {
            if (_shutDownRequested)
            {
                return;
            }
            //Create a collection which will block the thread and wait for new entries
            //if the collection is empty
            if (BufferSize.HasValue && BufferSize > 0)
            {
                _loggingEvents = new BlockingCollection<LoggingEventContext>(BufferSize.Value);
            }
            else
            {
                //No limit on the number of events.
                _loggingEvents = new BlockingCollection<LoggingEventContext>();
            }
            //The cancellation token is used to cancel a running task gracefully.
            _loggingCancelationTokenSource = new CancellationTokenSource();
            _loggingCancelationToken = _loggingCancelationTokenSource.Token;
            _loggingTask = new Task(SubscriberLoop, _loggingCancelationToken);
            _loggingTask.Start();
        }

        #endregion Startup

        #region Shutdown

        private void CompleteSubscriberTask()
        {
            _shutDownRequested = true;
            if (_loggingEvents == null || _loggingEvents.IsAddingCompleted)
            {
                return;
            }
            //Don't allow more entries to be added.
            _loggingEvents.CompleteAdding();
            //Allow some time to flush
            Thread.Sleep(_shutdownFlushTimespan);
            if (!_loggingTask.IsCompleted && !_loggingCancelationToken.IsCancellationRequested)
            {
                _loggingCancelationTokenSource.Cancel();
                //Wait here so that the error logging messages do not get into a random order.
                //Don't pass the cancellation token because we are not interested
                //in catching the OperationCanceledException that results.
                _loggingTask.Wait();
            }
            if (!_loggingEvents.IsCompleted)
            {
                ForwardInternalError("The buffer was not able to be flushed before timeout occurred.", null, ThisType);
            }
        }

        protected override void OnClose()
        {
            CompleteSubscriberTask();
            base.OnClose();
        }

        #endregion Shutdown

        #region Appending

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_loggingEvents == null || _loggingEvents.IsAddingCompleted || loggingEvent == null)
            {
                return;
            }

            loggingEvent.Fix = Fix;
            //In the case where blocking on a full collection, and the task is subsequently completed, the cancellation token
            //will prevent the entry from attempting to add to the completed collection which would result in an exception.
            _loggingEvents.Add(new LoggingEventContext(loggingEvent), _loggingCancelationToken);
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            if (_loggingEvents == null || _loggingEvents.IsAddingCompleted || loggingEvents == null)
            {
                return;
            }

            foreach (var loggingEvent in loggingEvents)
            {
                Append(loggingEvent);
            }
        }

        #endregion Appending

        #region Forwarding

        /// <summary>
        /// Iterates over a BlockingCollection containing LoggingEvents.
        /// </summary>
        private void SubscriberLoop()
        {
            Thread.CurrentThread.Name = String.Format("{0} ParallelForwardingAppender Subscriber Task", Name);
            //The task will continue in a blocking loop until
            //the queue is marked as adding completed, or the task is canceled.
            try
            {
                //This call blocks until an item is available or until adding is completed
                foreach (var entry in _loggingEvents.GetConsumingEnumerable(_loggingCancelationToken))
                {
                    ForwardLoggingEvent(entry.LoggingEvent, ThisType);
                }
            }
            catch (OperationCanceledException ex)
            {
                //The thread was canceled before all entries could be forwarded and the collection completed.
                ForwardInternalError("Subscriber task was canceled before completion.", ex, ThisType);
                //Cancellation is called in the CompleteSubscriberTask so don't call that again.
            }
            catch (ThreadAbortException ex)
            {
                //Thread abort may occur on domain unload.
                ForwardInternalError("Subscriber task was aborted.", ex, ThisType);
                //Cannot recover from a thread abort so complete the task.
                CompleteSubscriberTask();
                //The exception is swallowed because we don't want the client application
                //to halt due to a logging issue.
            }
            catch (Exception ex)
            {
                //On exception, try to log the exception
                ForwardInternalError("Subscriber task error in forwarding loop.", ex, ThisType);
                //Any error in the loop is going to be some sort of extenuating circumstance from which we
                //probably cannot recover anyway.   Complete subscribing.
                CompleteSubscriberTask();
            }
        }

        #endregion Forwarding

        #region IDisposable Implementation

        private bool _disposed = false;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_loggingTask != null)
                    {
                        if (!(_loggingTask.IsCanceled || _loggingTask.IsCompleted || _loggingTask.IsFaulted))
                        {
                            try
                            {
                                CompleteSubscriberTask();
                            }
                            catch (Exception ex)
                            {
                                LogLog.Error(ThisType, "Exception Completing Subscriber Task in Dispose Method", ex);
                            }
                        }
                        try
                        {
                            _loggingTask.Dispose();
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(ThisType, "Exception Disposing Logging Task", ex);
                        }
                        finally
                        {
                            _loggingTask = null;
                        }
                    }
                    if (_loggingEvents != null)
                    {
                        try
                        {
                            _loggingEvents.Dispose();
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(ThisType, "Exception Disposing BlockingCollection", ex);
                        }
                        finally
                        {
                            _loggingEvents = null;
                        }
                    }
                    if (_loggingCancelationTokenSource != null)
                    {
                        try
                        {
                            _loggingCancelationTokenSource.Dispose();
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(ThisType, "Exception Disposing CancellationTokenSource", ex);
                        }
                        finally
                        {
                            _loggingCancelationTokenSource = null;
                        }
                    }
                }
                _disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~ParallelForwardingAppender()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion IDisposable Implementation
    }
}