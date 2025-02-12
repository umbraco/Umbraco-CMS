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
    None = 0,

    /// <summary>
    /// Only publish the saving notification.
    /// </summary>
    Saving = 1,

    /// <summary>
    /// Only publish the saved notification.
    /// </summary>
    Saved = 2,

    /// <summary>
    /// Publish all the notifications.
    /// </summary>
    All = Saving | Saved,
}
