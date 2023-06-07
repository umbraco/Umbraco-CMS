using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "notificationModel", Namespace = "")]
public class SimpleNotificationModel : INotificationModel
{
    public SimpleNotificationModel() => Notifications = new List<BackOfficeNotification>();

    public SimpleNotificationModel(params BackOfficeNotification[] notifications) =>
        Notifications = new List<BackOfficeNotification>(notifications);

    /// <summary>
    ///     A default message
    /// </summary>
    [DataMember(Name = "message")]
    public string? Message { get; set; }

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }
}
