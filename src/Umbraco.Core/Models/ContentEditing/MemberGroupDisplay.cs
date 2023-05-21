using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "memberGroup", Namespace = "")]
public class MemberGroupDisplay : EntityBasic, INotificationModel
{
    public MemberGroupDisplay() => Notifications = new List<BackOfficeNotification>();

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }
}
