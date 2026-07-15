using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
/// Extension methods for <see cref="IContentRoutingReadiness"/>.
/// </summary>
public static class ContentRoutingReadinessExtensions
{
    /// <summary>
    /// Determines whether the application is running but per-server content caches are still being seeded,
    /// meaning front-end content must not be routed yet (a maintenance page / 503 should be shown instead).
    /// </summary>
    /// <remarks>
    /// Only the <see cref="RuntimeLevel.Run"/> window is considered here; other levels
    /// (<c>Install</c>, <c>Upgrade</c>, <c>Upgrading</c>, <c>BootFailed</c>) are already gated by the
    /// existing runtime-level checks in the routing and maintenance machinery.
    /// </remarks>
    public static bool IsInInitializationWindow(this IContentRoutingReadiness readiness, IRuntimeState runtimeState)
        => runtimeState.Level is RuntimeLevel.Run && readiness.IsReady is false;
}
