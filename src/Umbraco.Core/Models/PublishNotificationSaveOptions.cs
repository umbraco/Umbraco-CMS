namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Specifies options for publishing notifcations when saving.
/// </summary>
[Flags]
public enum PublishNotificationSaveOptions
{
    /// <summary>
    /// Do not publish any notifications.
    /// </summary>
    None,

    /// <summary>
    /// Only publish the saving notification.
    /// </summary>
    Saving,

    /// <summary>
    /// Only publish the saved notification.
    /// </summary>
    Saved,

    /// <summary>
    /// Publish all the notifications.
    /// </summary>
    All,
}
