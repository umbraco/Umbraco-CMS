namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs when Umbraco has completely booted up and the request processing pipeline is configured.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Notifications.IUmbracoApplicationLifetimeNotification" />
public class UmbracoApplicationStartedNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStartedNotification" /> class.
    /// </summary>
    /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
    public UmbracoApplicationStartedNotification(bool isRestarting) => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
