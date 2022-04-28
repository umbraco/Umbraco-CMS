using System;
using Serilog;
using Serilog.Extensions.Hosting;

namespace Umbraco.Cms.Web.Common.Logging;

/// <remarks>
/// HACK:
/// Ensures freeze is only called a single time even when resolving a logger from the snapshot container
/// built for <see cref="ModelsBuilder.RefreshingRazorViewEngine"/>.
/// </remarks>
internal class RegisteredReloadableLogger
{
    private static bool s_frozen;
    private static object s_frozenLock = new();
    private readonly ReloadableLogger _logger;

    public RegisteredReloadableLogger(ReloadableLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public ILogger Logger => _logger;

    public void Reload(Func<LoggerConfiguration, LoggerConfiguration> cfg)
    {
        lock (s_frozenLock)
        {
            if (s_frozen)
            {
                Logger.Debug("ReloadableLogger has already been frozen, unable to reload, NOOP.");
                return;
            }

            _logger.Reload(cfg);
            _logger.Freeze();

            s_frozen = true;
        }
    }
}

