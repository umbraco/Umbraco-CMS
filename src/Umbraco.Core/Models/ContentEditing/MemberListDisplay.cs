using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing a member list to be displayed in the back office
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class MemberListDisplay : ContentItemDisplayBase<ContentPropertyDisplay>
{
    [DataMember(Name = "apps")]
    public IEnumerable<ContentApp>? ContentApps { get; set; }
}
