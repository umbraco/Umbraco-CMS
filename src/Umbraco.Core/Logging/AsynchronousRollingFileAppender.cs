using log4net.Core;
using log4net.Util;
using System;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using log4net.Appender;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Based on https://github.com/cjbhaines/Log4Net.Async
    /// which is based on code by Chris Haines http://cjbhaines.wordpress.com/2012/02/13/asynchronous-log4net-appenders/
    /// This is an old/deprecated logger and has been superceded by ParallelForwardingAppender which is included in Umbraco and 
    /// also by AsyncForwardingAppender in the Log4Net.Async library.
	/// </summary>
	[Obsolete("This is superceded by the ParallelForwardingAppender, this will be removed in v8")]
    [EditorBrowsable(EditorBrowsableState.Never)]
	public class AsynchronousRollingFileAppender : RollingFileAppender
	{
		private RingBuffer<LoggingEvent> pendingAppends;
        private readonly ManualResetEvent manualResetEvent;
        private bool shuttingDown;
        private bool hasFinished;
        private bool forceStop;
        private bool logBufferOverflow;
        private int bufferOverflowCounter;
        private DateTime lastLoggedBufferOverflow;
        private int queueSizeLimit = 1000;
        public int QueueSizeLimit
        {
            get
            {
                return queueSizeLimit;
            }
            set
            {
                queueSizeLimit = value;
            }
        }

        public AsynchronousRollingFileAppender()
        {
            manualResetEvent = new ManualResetEvent(false);
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            pendingAppends = new RingBuffer<LoggingEvent>(QueueSizeLimit);
            pendingAppends.BufferOverflow += OnBufferOverflow;
            StartAppendTask();
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            Array.ForEach(loggingEvents, Append);
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
            {
                pendingAppends.Enqueue(loggingEvent);
            }
        }

        protected override void OnClose()
        {
            shuttingDown = true;
            manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));

            if (!hasFinished)
            {
                forceStop = true;
                base.Append(new LoggingEvent(new LoggingEventData
                {
                    Level = Level.Error,
                    Message = "Unable to clear out the AsyncRollingFileAppender buffer in the allotted time, forcing a shutdown",
                    TimeStamp = DateTime.UtcNow,
                    Identity = "",
                    ExceptionString = "",
                    UserName = WindowsIdentity.GetCurrent() != null ? WindowsIdentity.GetCurrent().Name : "",
                    Domain = AppDomain.CurrentDomain.FriendlyName,
                    ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                    LocationInfo = new LocationInfo(this.GetType().Name, "OnClose", "AsyncRollingFileAppender.cs", "75"),
                    LoggerName = this.GetType().FullName,
                    Properties = new PropertiesDictionary(),
                })
                 );
            }

            base.OnClose();
        }

        private void StartAppendTask()
        {
            if (!shuttingDown)
            {
                Task appendTask = new Task(AppendLoggingEvents, TaskCreationOptions.LongRunning);
                appendTask.LogErrors(LogAppenderError).ContinueWith(x => StartAppendTask()).LogErrors(LogAppenderError);
                appendTask.Start();
            }
        }

        private void LogAppenderError(string logMessage, Exception exception)
        {
            base.Append(new LoggingEvent(new LoggingEventData
            {
                Level = Level.Error,
                Message = "Appender exception: " + logMessage,
                TimeStamp = DateTime.UtcNow,
                Identity = "",
                ExceptionString = exception.ToString(),
                UserName = WindowsIdentity.GetCurrent() != null ? WindowsIdentity.GetCurrent().Name : "",
                Domain = AppDomain.CurrentDomain.FriendlyName,
                ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                LocationInfo = new LocationInfo(this.GetType().Name, "LogAppenderError", "AsyncRollingFileAppender.cs", "152"),
                LoggerName = this.GetType().FullName,
                Properties = new PropertiesDictionary(),
            }));
        }

        private void AppendLoggingEvents()
        {
            LoggingEvent loggingEventToAppend;
            while (!shuttingDown)
            {
                if (logBufferOverflow)
                {
                    LogBufferOverflowError();
                    logBufferOverflow = false;
                    bufferOverflowCounter = 0;
                    lastLoggedBufferOverflow = DateTime.UtcNow;
                }

                while (!pendingAppends.TryDequeue(out loggingEventToAppend))
                {
                    Thread.Sleep(10);
                    if (shuttingDown)
                    {
                        break;
                    }
                }
                if (loggingEventToAppend == null)
                {
                    continue;
                }

                try
                {
                    base.Append(loggingEventToAppend);
                }
                catch
                {
                }
            }

            while (pendingAppends.TryDequeue(out loggingEventToAppend) && !forceStop)
            {
                try
                {
                    base.Append(loggingEventToAppend);
                }
                catch
                {
                }
            }
            hasFinished = true;
            manualResetEvent.Set();
        }

        private void LogBufferOverflowError()
        {
            base.Append(new LoggingEvent(new LoggingEventData
            {
                Level = Level.Error,
                Message = string.Format("Buffer overflow. {0} logging events have been lost in the last 30 seconds. [QueueSizeLimit: {1}]", bufferOverflowCounter, QueueSizeLimit),
                TimeStamp = DateTime.UtcNow,
                Identity = "",
                ExceptionString = "",
                UserName = WindowsIdentity.GetCurrent() != null ? WindowsIdentity.GetCurrent().Name : "",
                Domain = AppDomain.CurrentDomain.FriendlyName,
                ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                LocationInfo = new LocationInfo(this.GetType().Name, "LogBufferOverflowError", "AsyncRollingFileAppender.cs", "152"),
                LoggerName = this.GetType().FullName,
                Properties = new PropertiesDictionary(),
            }));
        }

        private void OnBufferOverflow(object sender, EventArgs eventArgs)
        {
            bufferOverflowCounter++;
            if (logBufferOverflow == false)
            {
                if (lastLoggedBufferOverflow < DateTime.UtcNow.AddSeconds(-30))
                {
                    logBufferOverflow = true;
                }
            }
        }
    }

}