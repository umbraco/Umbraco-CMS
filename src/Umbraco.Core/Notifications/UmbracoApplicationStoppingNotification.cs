namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Notification that occurs when Umbraco is shutting down (after all components are terminated).
/// </summary>
public class UmbracoApplicationStoppingNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoApplicationStoppingNotification" /> class.
    /// </summary>
    /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
    public UmbracoApplicationStoppingNotification(bool isRestarting)
        => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
