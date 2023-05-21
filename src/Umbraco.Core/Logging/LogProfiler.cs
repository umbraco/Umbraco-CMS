using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Implements <see cref="IProfiler" /> by writing profiling results to an <see cref="ILogger" />.
/// </summary>
public class LogProfiler : IProfiler
{
    private readonly ILogger<LogProfiler> _logger;

    public LogProfiler(ILogger<LogProfiler> logger) => _logger = logger;

    /// <inheritdoc />
    public IDisposable Step(string name)
    {
        _logger.LogDebug("Begin: {ProfileName}", name);
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

    // a lightweight disposable timer
    private class LightDisposableTimer : DisposableObjectSlim
    {
        private readonly Action<long> _callback;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        protected internal LightDisposableTimer(Action<long> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        protected override void DisposeResources()
        {
            _stopwatch.Stop();
            _callback(_stopwatch.ElapsedMilliseconds);
        }
    }
}
