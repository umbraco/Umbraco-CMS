using Serilog;
using Serilog.Extensions.Hosting;

namespace Umbraco.Cms.Web.Common.Logging;

/// <remarks>
///     HACK:
///     Ensures freeze is only called a single time even when resolving a logger from a snapshot container.
/// </remarks>
internal sealed class RegisteredReloadableLogger
{
    private static readonly Lock _frozenLock = new();
    private static bool _frozen;
    private readonly ReloadableLogger _logger;

    public RegisteredReloadableLogger(ReloadableLogger? logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public ILogger Logger => _logger;

    public void Reload(Func<LoggerConfiguration, LoggerConfiguration> cfg)
    {
        lock (_frozenLock)
        {
            if (_frozen)
            {
                Logger.Debug("ReloadableLogger has already been frozen, unable to reload, NOOP.");
                return;
            }

            _logger.Reload(cfg);
            _logger.Freeze();

            _frozen = true;
        }
    }
}
