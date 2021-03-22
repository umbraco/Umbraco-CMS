using System;
using System.Diagnostics;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Starts the timer and invokes a  callback upon disposal. Provides a simple way of timing an operation by wrapping it in a <code>using</code> (C#) statement.
    /// </summary>
	public class DisposableTimer : DisposableObjectSlim
    {
        private readonly ILogger _logger;
        private readonly LogLevel _level;
        private readonly Type _loggerType;
        private readonly int _thresholdMilliseconds;
        private readonly IDisposable _profilerStep;
        private readonly string _endMessage;
        private string _failMessage;
        private Exception _failException;
        private bool _failed;
        private readonly string _timingId;

        // internal - created by profiling logger
        internal DisposableTimer(ILogger logger, LogLevel level, IProfiler profiler, Type loggerType,
            string startMessage, string endMessage, string failMessage = null,
            int thresholdMilliseconds = 0)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _level = level;
            _loggerType = loggerType ?? throw new ArgumentNullException(nameof(loggerType));
            _endMessage = endMessage;
            _failMessage = failMessage;
            _thresholdMilliseconds = thresholdMilliseconds < 0 ? 0 : thresholdMilliseconds;
            _timingId = Guid.NewGuid().ToString("N").Substring(0, 7); // keep it short-ish

            if (thresholdMilliseconds == 0)
            {
                switch (_level)
                {
                    case LogLevel.Debug:
                        logger.Debug<string,string>(loggerType, "{StartMessage} [Timing {TimingId}]", startMessage, _timingId);
                        break;
                    case LogLevel.Information:
                        logger.Info<string, string>(loggerType, "{StartMessage} [Timing {TimingId}]", startMessage, _timingId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level));
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
                && _loggerType != null && _logger != null
                && (_endMessage.IsNullOrWhiteSpace() == false || _failed))
            {
                if (_failed)
                {
                    _logger.Error<string,long,string>(_loggerType, _failException, "{FailMessage} ({Duration}ms) [Timing {TimingId}]", _failMessage, Stopwatch.ElapsedMilliseconds, _timingId);
                }
                else switch (_level)
                {
                    case LogLevel.Debug:
                        _logger.Debug<string,long,string>(_loggerType, "{EndMessage} ({Duration}ms) [Timing {TimingId}]", _endMessage, Stopwatch.ElapsedMilliseconds, _timingId);
                        break;
                    case LogLevel.Information:
                        _logger.Info<string, long, string>(_loggerType, "{EndMessage} ({Duration}ms) [Timing {TimingId}]", _endMessage, Stopwatch.ElapsedMilliseconds, _timingId);
                        break;
                    // filtered in the ctor
                    //default:
                    //    throw new Exception();
                }
            }
        }
    }
}
