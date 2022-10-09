namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Represents an Umbraco application lifetime (starting, started, stopping, stopped) notification.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Notifications.INotification" />
public interface IUmbracoApplicationLifetimeNotification : INotification
{
    /// <summary>
    ///     Gets a value indicating whether Umbraco is restarting (e.g. after an install or upgrade).
    /// </summary>
    /// <value>
    ///     <c>true</c> if Umbraco is restarting; otherwise, <c>false</c>.
    /// </value>
    bool IsRestarting { get; }
}
