// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs when Umbraco is shutting down (after all components are terminated).
/// </summary>
/// <remarks>
///     This notification is published during the application shutdown phase.
///     Use this notification to perform cleanup tasks before Umbraco stops.
/// </remarks>
public class UmbracoApplicationStoppingNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStoppingNotification"/> class.
    /// </summary>
    /// <param name="isRestarting">A value indicating whether Umbraco is restarting.</param>
    public UmbracoApplicationStoppingNotification(bool isRestarting)
        => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
