using System;
using System.Diagnostics;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Implements <see cref="IProfiler"/> by writing profiling results to an <see cref="ILogger"/>.
    /// </summary>
    internal class LogProfiler : IProfiler
    {
        private readonly ILogger _logger;

        public LogProfiler(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Render()
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public IDisposable Step(string name)
        {
            _logger.Debug<LogProfiler, string>("Begin: {ProfileName}", name);
            return new LightDisposableTimer(duration => _logger.Info<LogProfiler, string,long>("End {ProfileName} ({ProfileDuration}ms)", name, duration));
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
