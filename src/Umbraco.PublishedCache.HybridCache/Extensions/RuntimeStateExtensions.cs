using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IRuntimeState"/> used by the cache startup notification handlers.
/// </summary>
internal static class RuntimeStateExtensions
{
    /// <summary>
    /// Returns true when startup cache seeding should be skipped because the site is not yet serving
    /// front-end content, i.e. it is installing (or below) or upgrading with the maintenance page shown.
    /// </summary>
    /// <param name="state">The runtime state.</param>
    /// <param name="globalSettings">The global settings.</param>
    public static bool ShouldSkipStartupSeeding(this IRuntimeState state, GlobalSettings globalSettings)
        => state.Level <= RuntimeLevel.Install
           || (state.Level == RuntimeLevel.Upgrade && globalSettings.ShowMaintenancePageWhenInUpgradeState);
}
