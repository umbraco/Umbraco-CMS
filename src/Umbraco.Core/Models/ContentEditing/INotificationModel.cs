using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public interface INotificationModel
{
    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    List<BackOfficeNotification>? Notifications { get; }
}
