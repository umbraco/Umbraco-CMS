﻿using System;
using System.Diagnostics;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Starts the timer and invokes a  callback upon disposal. Provides a simple way of timing an operation by wrapping it in a <code>using</code> (C#) statement.
    /// </summary>
	public class DisposableTimer : DisposableObjectSlim
    {
        private readonly ILogger _logger;
        private readonly LogType? _logType;
        private readonly Type _loggerType;
        private readonly int _thresholdMilliseconds;
        private readonly IDisposable _profilerStep;
        private readonly string _endMessage;
        private string _failMessage;
        private Exception _failException;
        private bool _failed;

        internal enum LogType
        {
            Debug, Info
        }

        // internal - created by profiling logger
        internal DisposableTimer(ILogger logger, LogType logType, IProfiler profiler, Type loggerType,
            string startMessage, string endMessage, string failMessage = null,
            int thresholdMilliseconds = 0)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (loggerType == null) throw new ArgumentNullException(nameof(loggerType));

            _logger = logger;
            _logType = logType;
            _loggerType = loggerType;
            _endMessage = endMessage;
            _failMessage = failMessage;
            _thresholdMilliseconds = thresholdMilliseconds < 0 ? 0 : thresholdMilliseconds;

            if (thresholdMilliseconds == 0)
            {
                switch (logType)
                {
                    case LogType.Debug:
                        logger.Debug(loggerType, startMessage);
                        break;
                    case LogType.Info:
                        logger.Info(loggerType, startMessage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logType));
                }
            }

            // else aren't logging the start message, this is output to the profiler but not the log,
            // we just want the log to contain the result if it's more than the minimum ms threshold.

            _profilerStep = profiler?.Step(loggerType, startMessage);
        }

        /// <summary>
        /// Reports a failure.
        /// </summary>
        /// <param name="failMessage">The fail message.</param>
        /// <param name="exception">The exception.</param>
        /// <remarks>Completion of the timer will be reported as an error, with the specified message and exception.</remarks>
        public void Fail(string failMessage = null, Exception exception = null)
        {
            _failed = true;
            _failMessage = failMessage ?? _failMessage ?? "Failed.";
            _failException = exception;
        }

        public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

        /// <summary>
        ///Disposes resources.
        /// </summary>
        /// <remarks>Overrides abstract class <see cref="DisposableObject"/> which handles required locking.</remarks>
        protected override void DisposeResources()
        {
            Stopwatch.Stop();

            _profilerStep?.Dispose();

            if ((Stopwatch.ElapsedMilliseconds >= _thresholdMilliseconds || _failed)
                && _logType.HasValue && _loggerType != null && _logger != null
                && (_endMessage.IsNullOrWhiteSpace() == false || _failed))
            {
                if (_failed)
                {
                    _logger.Error(_loggerType, $"{_failMessage} ({Stopwatch.ElapsedMilliseconds}ms)", _failException);
                }
                else switch (_logType)
                {
                    case LogType.Debug:
                        _logger.Debug(_loggerType, () => $"{_endMessage} ({Stopwatch.ElapsedMilliseconds}ms)");
                        break;
                    case LogType.Info:
                        _logger.Info(_loggerType, () => $"{_endMessage} ({Stopwatch.ElapsedMilliseconds}ms)");
                        break;
                    // filtered in the ctor
                    //default:
                    //    throw new Exception();
                }
            }
        }
    }
}
