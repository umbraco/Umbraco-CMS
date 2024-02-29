using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing a member to be displayed in the back office
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class MemberDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay>
{

    [DataMember(Name = "contentType")]
    public ContentTypeBasic? ContentType { get; set; }

    [DataMember(Name = "username")]
    public string? Username { get; set; }

    [DataMember(Name = "email")]
    public string? Email { get; set; }

    [DataMember(Name = "isLockedOut")]
    public bool IsLockedOut { get; set; }

    [DataMember(Name = "isApproved")]
    public bool IsApproved { get; set; }

    [DataMember(Name = "membershipProperties")]
    public IEnumerable<ContentPropertyDisplay>? MembershipProperties { get; set; }
}
