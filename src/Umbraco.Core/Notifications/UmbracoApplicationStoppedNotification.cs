// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs when Umbraco has completely shutdown.
/// </summary>
/// <remarks>
///     This notification is published after Umbraco has fully stopped.
///     Use this notification for final cleanup or logging after shutdown.
/// </remarks>
/// <seealso cref="IUmbracoApplicationLifetimeNotification"/>
public class UmbracoApplicationStoppedNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStoppedNotification"/> class.
    /// </summary>
    /// <param name="isRestarting">A value indicating whether Umbraco is restarting.</param>
    public UmbracoApplicationStoppedNotification(bool isRestarting) => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
