// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs at the very end of the Umbraco boot process (after all components are initialized).
/// </summary>
/// <remarks>
///     This notification is published during the application startup phase, before the request pipeline is fully configured.
///     Use <see cref="UmbracoApplicationStartedNotification"/> if you need to perform actions after Umbraco is fully started.
/// </remarks>
public class UmbracoApplicationStartingNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStartingNotification"/> class.
    /// </summary>
    /// <param name="runtimeLevel">The current runtime level of the application.</param>
    /// <param name="isRestarting">A value indicating whether Umbraco is restarting.</param>
    public UmbracoApplicationStartingNotification(RuntimeLevel runtimeLevel, bool isRestarting)
    {
        RuntimeLevel = runtimeLevel;
        IsRestarting = isRestarting;
    }

    /// <summary>
    ///     Gets the current runtime level of the application.
    /// </summary>
    public RuntimeLevel RuntimeLevel { get; }

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
