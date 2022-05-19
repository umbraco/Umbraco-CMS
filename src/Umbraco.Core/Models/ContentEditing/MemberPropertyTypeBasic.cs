using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Basic member property type
/// </summary>
[DataContract(Name = "contentType", Namespace = "")]
public class MemberPropertyTypeBasic : PropertyTypeBasic
{
    [DataMember(Name = "showOnMemberProfile")]
    public bool MemberCanViewProperty { get; set; }

    [DataMember(Name = "memberCanEdit")]
    public bool MemberCanEditProperty { get; set; }

    [DataMember(Name = "isSensitiveData")]
    public bool IsSensitiveData { get; set; }
}
