using System.ComponentModel;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The user model used for paging and listing users in the UI
/// </summary>
[DataContract(Name = "user", Namespace = "")]
[ReadOnly(true)]
public class UserBasic : EntityBasic, INotificationModel
{
    public UserBasic()
    {
        Notifications = new List<BackOfficeNotification>();
        UserGroups = new List<UserGroupBasic>();
    }

    [DataMember(Name = "username")]
    public string? Username { get; set; }

    /// <summary>
    ///     The MD5 lowercase hash of the email which can be used by gravatar
    /// </summary>
    [DataMember(Name = "emailHash")]
    public string? EmailHash { get; set; }

    [DataMember(Name = "lastLoginDate")]
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    ///     Returns a list of different size avatars
    /// </summary>
    [DataMember(Name = "avatars")]
    public string[]? Avatars { get; set; }

    [DataMember(Name = "userState")]
    public UserState UserState { get; set; }

    [DataMember(Name = "culture", IsRequired = true)]
    public string? Culture { get; set; }

    [DataMember(Name = "email", IsRequired = true)]
    public string? Email { get; set; }

    /// <summary>
    ///     The list of group aliases assigned to the user
    /// </summary>
    [DataMember(Name = "userGroups")]
    public IEnumerable<UserGroupBasic> UserGroups { get; set; }

    /// <summary>
    ///     This is an info flag to denote if this object is the equivalent of the currently logged in user
    /// </summary>
    [DataMember(Name = "isCurrentUser")]
    [ReadOnly(true)]
    public bool IsCurrentUser { get; set; }

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }
}
