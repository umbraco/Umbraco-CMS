using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace Umbraco.Core.Logging
{
	/// <summary>
	/// Based on code by Chris Haines http://cjbhaines.wordpress.com/2012/02/13/asynchronous-log4net-appenders/
	/// </summary>
	public class AsynchronousRollingFileAppender : RollingFileAppender
	{
		private readonly ManualResetEvent _manualResetEvent;
		private int _bufferOverflowCounter;
		private bool _forceStop;
		private bool _hasFinished;
		private DateTime _lastLoggedBufferOverflow;
		private bool _logBufferOverflow;
		private RingBuffer<LoggingEvent> _pendingAppends;
		private int _queueSizeLimit = 1000;
		private bool _shuttingDown;

		public AsynchronousRollingFileAppender()
		{
			_manualResetEvent = new ManualResetEvent(false);
		}

		public int QueueSizeLimit
		{
			get { return _queueSizeLimit; }
			set { _queueSizeLimit = value; }
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			_pendingAppends = new RingBuffer<LoggingEvent>(QueueSizeLimit);
			_pendingAppends.BufferOverflow += OnBufferOverflow;
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
				_pendingAppends.Enqueue(loggingEvent);
			}
		}

		protected override void OnClose()
		{
			_shuttingDown = true;
			_manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));

			if (!_hasFinished)
			{
				_forceStop = true;
				var windowsIdentity = WindowsIdentity.GetCurrent();
				base.Append(new LoggingEvent(new LoggingEventData
					{
						Level = Level.Error,
						Message =
							"Unable to clear out the AsynchronousRollingFileAppender buffer in the allotted time, forcing a shutdown",
						TimeStamp = DateTime.UtcNow,
						Identity = "",
						ExceptionString = "",
						UserName = windowsIdentity != null ? windowsIdentity.Name : "",
						Domain = AppDomain.CurrentDomain.FriendlyName,
						ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
						LocationInfo =
							new LocationInfo(this.GetType().Name, "OnClose", "AsynchronousRollingFileAppender.cs", "59"),
						LoggerName = this.GetType().FullName,
						Properties = new PropertiesDictionary(),
					})
					);
			}

			base.OnClose();
		}

		private void StartAppendTask()
		{
			if (!_shuttingDown)
			{
				Task appendTask = new Task(AppendLoggingEvents, TaskCreationOptions.LongRunning);
				appendTask.LogErrors(LogAppenderError).ContinueWith(x => StartAppendTask()).LogErrors(LogAppenderError);
				appendTask.Start();
			}
		}

		private void LogAppenderError(string logMessage, Exception exception)
		{
			var windowsIdentity = WindowsIdentity.GetCurrent();
			base.Append(new LoggingEvent(new LoggingEventData
				{
					Level = Level.Error,
					Message = "Appender exception: " + logMessage,
					TimeStamp = DateTime.UtcNow,
					Identity = "",
					ExceptionString = exception.ToString(),
					UserName = windowsIdentity != null ? windowsIdentity.Name : "",
					Domain = AppDomain.CurrentDomain.FriendlyName,
					ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
					LocationInfo =
						new LocationInfo(this.GetType().Name,
						                 "LogAppenderError",
						                 "AsynchronousRollingFileAppender.cs",
						                 "100"),
					LoggerName = this.GetType().FullName,
					Properties = new PropertiesDictionary(),
				}));
		}

		private void AppendLoggingEvents()
		{
			LoggingEvent loggingEventToAppend;
			while (!_shuttingDown)
			{
				if (_logBufferOverflow)
				{
					LogBufferOverflowError();
					_logBufferOverflow = false;
					_bufferOverflowCounter = 0;
					_lastLoggedBufferOverflow = DateTime.UtcNow;
				}

				while (!_pendingAppends.TryDequeue(out loggingEventToAppend))
				{
					Thread.Sleep(10);
					if (_shuttingDown)
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

			while (_pendingAppends.TryDequeue(out loggingEventToAppend) && !_forceStop)
			{
				try
				{
					base.Append(loggingEventToAppend);
				}
				catch
				{
				}
			}
			_hasFinished = true;
			_manualResetEvent.Set();
		}

		private void LogBufferOverflowError()
		{
			var windowsIdentity = WindowsIdentity.GetCurrent();
			base.Append(new LoggingEvent(new LoggingEventData
				{
					Level = Level.Error,
					Message =
						string.Format(
							"Buffer overflow. {0} logging events have been lost in the last 30 seconds. [QueueSizeLimit: {1}]",
							_bufferOverflowCounter,
							QueueSizeLimit),
					TimeStamp = DateTime.UtcNow,
					Identity = "",
					ExceptionString = "",
					UserName = windowsIdentity != null ? windowsIdentity.Name : "",
					Domain = AppDomain.CurrentDomain.FriendlyName,
					ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
					LocationInfo =
						new LocationInfo(this.GetType().Name,
						                 "LogBufferOverflowError",
						                 "AsynchronousRollingFileAppender.cs",
						                 "172"),
					LoggerName = this.GetType().FullName,
					Properties = new PropertiesDictionary(),
				}));
		}

		private void OnBufferOverflow(object sender, EventArgs eventArgs)
		{
			_bufferOverflowCounter++;
			if (_logBufferOverflow == false)
			{
				if (_lastLoggedBufferOverflow < DateTime.UtcNow.AddSeconds(-30))
				{
					_logBufferOverflow = true;
				}
			}
		}

		private class RingBuffer<T>
		{
			private readonly object _lockObject = new object();
			private readonly T[] _buffer;
			private readonly int _size;
			private int _readIndex = 0;
			private int _writeIndex = 0;
			private bool _bufferFull = false;

			public event Action<object, EventArgs> BufferOverflow;

			public RingBuffer(int size)
			{
				this._size = size;
				_buffer = new T[size];
			}

			public void Enqueue(T item)
			{
				lock (_lockObject)
				{
					_buffer[_writeIndex] = item;
					_writeIndex = (++_writeIndex) % _size;
					if (_bufferFull)
					{
						if (BufferOverflow != null)
						{
							BufferOverflow(this, EventArgs.Empty);
						}
						_readIndex = _writeIndex;
					}
					else if (_writeIndex == _readIndex)
					{
						_bufferFull = true;
					}
				}
			}

			public bool TryDequeue(out T ret)
			{
				if (_readIndex == _writeIndex && !_bufferFull)
				{
					ret = default(T);
					return false;
				}
				lock (_lockObject)
				{
					if (_readIndex == _writeIndex && !_bufferFull)
					{
						ret = default(T);
						return false;
					}

					ret = _buffer[_readIndex];
					_readIndex = (++_readIndex) % _size;
					_bufferFull = false;
					return true;
				}
			}
		}
	}
}