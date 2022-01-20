namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs when Umbraco is shutting down (after all <see cref="IComponent" />s are terminated).
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Notifications.INotification" />
    public class UmbracoApplicationStoppingNotification : INotification
    { }
}
