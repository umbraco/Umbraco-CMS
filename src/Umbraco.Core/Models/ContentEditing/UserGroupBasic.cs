using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "userGroup", Namespace = "")]
public class UserGroupBasic : EntityBasic, INotificationModel
{
    public UserGroupBasic()
    {
        Notifications = new List<BackOfficeNotification>();
        Languages = Enumerable.Empty<Language>();
        Sections = Enumerable.Empty<Section>();
    }

    [DataMember(Name = "languages")]
    public IEnumerable<Language> Languages { get; set; }

    [DataMember(Name = "sections")]
    public IEnumerable<Section> Sections { get; set; }

    [DataMember(Name = "contentStartNode")]
    public EntityBasic? ContentStartNode { get; set; }

    [DataMember(Name = "mediaStartNode")]
    public EntityBasic? MediaStartNode { get; set; }

    [DataMember(Name = "hasAccessToAllLanguages")]
    public bool HasAccessToAllLanguages { get; set; }

    /// <summary>
    ///     The number of users assigned to this group
    /// </summary>
    [DataMember(Name = "userCount")]
    public int UserCount { get; set; }

    /// <summary>
    ///     Is the user group a system group e.g. "Administrators", "Sensitive data" or "Translators"
    /// </summary>
    [DataMember(Name = "isSystemUserGroup")]
    public bool IsSystemUserGroup { get; set; }

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }
}
