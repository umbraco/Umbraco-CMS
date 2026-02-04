using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Implements <see cref="IProfiler" /> by writing profiling results to an <see cref="ILogger" />.
/// </summary>
public class LogProfiler : IProfiler
{
    private readonly ILogger<LogProfiler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogProfiler"/> class.
    /// </summary>
    /// <param name="logger">The logger to write profiling results to.</param>
    public LogProfiler(ILogger<LogProfiler> logger) => _logger = logger;

    /// <inheritdoc />
    public IDisposable Step(string name)
    {
        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            _logger.LogDebug("Begin: {ProfileName}", name);
        }
        return new LightDisposableTimer(duration =>
            _logger.LogInformation("End {ProfileName} ({ProfileDuration}ms)", name, duration));
    }

    /// <inheritdoc />
    public void Start()
    {
        // the log will always be started
    }

    /// <inheritdoc />
    public void Stop(bool discardResults = false)
    {
        // the log never stops
    }

    /// <inheritdoc />
    public bool IsEnabled => _logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug);

    /// <summary>
    ///     A lightweight disposable timer that invokes a callback with the elapsed time upon disposal.
    /// </summary>
    private sealed class LightDisposableTimer : DisposableObjectSlim
    {
        private readonly Action<long> _callback;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        /// <summary>
        ///     Initializes a new instance of the <see cref="LightDisposableTimer"/> class.
        /// </summary>
        /// <param name="callback">The callback to invoke with the elapsed milliseconds when disposed.</param>
        internal LightDisposableTimer(Action<long> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        protected override void DisposeResources()
        {
            _stopwatch.Stop();
            _callback(_stopwatch.ElapsedMilliseconds);
        }
    }
}
