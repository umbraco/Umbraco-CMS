// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
/// Makes one bounded, best-effort attempt to elect the current server's role, for callers that need to resolve
/// it before publishing <see cref="Core.Notifications.UmbracoApplicationStartingNotification"/> - both the normal
/// boot path (<see cref="Runtime.CoreRuntime"/>) and the unattended-upgrade background path
/// (<see cref="Install.UnattendedUpgradeBackgroundService"/>), which publishes that notification itself once
/// migrations complete rather than going through <see cref="Runtime.CoreRuntime"/>.
/// </summary>
internal interface IServerRoleElector
{
    /// <summary>
    ///     Makes one bounded, best-effort attempt to elect this server's role before it can be observed as
    ///     <see cref="ServerRole.Unknown" /> by any <see cref="Core.Notifications.UmbracoApplicationStartingNotification" />
    ///     handler.
    /// </summary>
    /// <remarks>
    ///     No-ops unless the registered <see cref="IServerRoleAccessor" /> is the default
    ///     <see cref="ElectedServerRoleAccessor" /> - a custom accessor supplied via
    ///     <c>IUmbracoBuilder.SetServerRegistrar{T}()</c>, or <c>DisableElectionForSingleServer</c>, means there is
    ///     nothing here for this server to elect. Never throws: a timeout or a genuinely read-only database just
    ///     leaves the role as <see cref="ServerRole.Unknown" />, exactly as it would without this attempt.
    /// </remarks>
    Task TryElectOnceAsync(CancellationToken cancellationToken);
}
