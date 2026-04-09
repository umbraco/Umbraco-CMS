// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after runtime pre-migrations have been applied during upgrade.
/// </summary>
/// <remarks>
///     This notification is published after the pre-migration upgrade steps have completed,
///     allowing handlers to perform post-upgrade actions.
/// </remarks>
public class PostRuntimePremigrationsUpgradeNotification : INotification
{
}
