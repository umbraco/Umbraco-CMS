// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs when Umbraco has completely booted up and the request processing pipeline is configured.
/// </summary>
/// <remarks>
///     This notification is published after Umbraco has fully started and is ready to process requests.
///     Use this notification for initialization tasks that depend on Umbraco being fully operational.
/// </remarks>
/// <seealso cref="IUmbracoApplicationLifetimeNotification"/>
public class UmbracoApplicationStartedNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStartedNotification"/> class.
    /// </summary>
    /// <param name="isRestarting">A value indicating whether Umbraco is restarting.</param>
    public UmbracoApplicationStartedNotification(bool isRestarting) => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
