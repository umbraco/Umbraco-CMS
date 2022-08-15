namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs when Umbraco has completely shutdown.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Notifications.IUmbracoApplicationLifetimeNotification" />
public class UmbracoApplicationStoppedNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStoppedNotification" /> class.
    /// </summary>
    /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
    public UmbracoApplicationStoppedNotification(bool isRestarting) => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
