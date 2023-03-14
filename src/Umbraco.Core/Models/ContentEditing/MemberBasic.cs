using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Used for basic member information
/// </summary>
public class MemberBasic : ContentItemBasic<ContentPropertyBasic>
{
    [DataMember(Name = "username")]
    public string? Username { get; set; }

    [DataMember(Name = "email")]
    public string? Email { get; set; }

    [DataMember(Name = "properties")]
    public override IEnumerable<ContentPropertyBasic> Properties
    {
        get => base.Properties;
        set => base.Properties = value;
    }
}
