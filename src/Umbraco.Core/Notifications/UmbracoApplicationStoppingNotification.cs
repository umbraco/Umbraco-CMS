namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that occurs when Umbraco is shutting down (after all <see cref="IComponent" />s are terminated).
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Notifications.IUmbracoApplicationLifetimeNotification" />
public class UmbracoApplicationStoppingNotification : IUmbracoApplicationLifetimeNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStoppingNotification" /> class.
    /// </summary>
    [Obsolete("Use ctor with all params")]
    public UmbracoApplicationStoppingNotification()
        : this(false)
    {
        // TODO: Remove this constructor in V10
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationStoppingNotification" /> class.
    /// </summary>
    /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
    public UmbracoApplicationStoppingNotification(bool isRestarting) => IsRestarting = isRestarting;

    /// <inheritdoc />
    public bool IsRestarting { get; }
}
