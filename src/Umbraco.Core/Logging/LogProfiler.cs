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

        [LoggerMessage(
           EventId = 8,
           Level = MicrosoftLogLevel.Debug,
           Message = "Begin: {ProfileName}")]
        public partial void LogStartStep(string name);

        [LoggerMessage(
           EventId = 9,
           Level = MicrosoftLogLevel.Information,
           Message = "End {ProfileName} ({ProfileDuration}ms)")]
        public partial void LogEndStep(string name, long duration);

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
