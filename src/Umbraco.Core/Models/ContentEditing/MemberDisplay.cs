using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing a member to be displayed in the back office
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class MemberDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay>
{
    public MemberDisplay() =>

        // MemberProviderFieldMapping = new Dictionary<string, string>();
        ContentApps = new List<ContentApp>();

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

    // [DataMember(Name = "membershipScenario")]
    // public MembershipScenario MembershipScenario { get; set; }

    // /// <summary>
    // /// This is used to indicate how to map the membership provider properties to the save model, this mapping
    // /// will change if a developer has opted to have custom member property aliases specified in their membership provider config,
    // /// or if we are editing a member that is not an Umbraco member (custom provider)
    // /// </summary>
    // [DataMember(Name = "fieldConfig")]
    // public IDictionary<string, string> MemberProviderFieldMapping { get; set; }
    [DataMember(Name = "apps")]
    public IEnumerable<ContentApp> ContentApps { get; set; }

    [DataMember(Name = "membershipProperties")]
    public IEnumerable<ContentPropertyDisplay>? MembershipProperties { get; set; }
}
