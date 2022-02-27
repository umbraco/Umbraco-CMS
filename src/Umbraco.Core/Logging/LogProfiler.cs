using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Umbraco.Cms.Core.Logging
{
    /// <summary>
    /// Implements <see cref="IProfiler"/> by writing profiling results to an <see cref="ILogger"/>.
    /// </summary>
    public partial class LogProfiler : IProfiler
    {
        private readonly ILogger<LogProfiler> _logger;
        private static readonly Action<ILogger, string, Exception> s_logStartStep
           = LoggerMessage.Define<string>(MicrosoftLogLevel.Debug, new EventId(8), "Begin: {ProfileName}");

        private static readonly Action<ILogger, string, long,Exception> s_logEndStep
           = LoggerMessage.Define<string,long>(MicrosoftLogLevel.Debug, new EventId(9), "End {ProfileName} ({ProfileDuration}ms)");

        public LogProfiler(ILogger<LogProfiler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IDisposable Step(string name)
        {
            LogStartStep(name);
            return new LightDisposableTimer(duration => LogEndStep(name, duration));
        }

        /// <inheritdoc/>
        public void Start()
        {
            // the log will always be started
        }

        /// <inheritdoc/>
        public void Stop(bool discardResults = false)
        {
            // the log never stops
        }

        private void LogStartStep(string profileName)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logStartStep.Invoke(_logger, profileName, null);
            }
        }

        private void LogEndStep(string profileName, long profileDuration)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logEndStep.Invoke(_logger, profileName, profileDuration, null);
            }
        }

        // a lightweight disposable timer
        private class LightDisposableTimer : DisposableObjectSlim
        {
            private readonly Action<long> _callback;
            private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

            protected internal LightDisposableTimer(Action<long> callback)
            {
                if (callback == null) throw new ArgumentNullException(nameof(callback));
                _callback = callback;
            }

            protected override void DisposeResources()
            {
                _stopwatch.Stop();
                _callback(_stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
