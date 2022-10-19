using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Used to unpublish content and variants
/// </summary>
[DataContract(Name = "unpublish", Namespace = "")]
public class UnpublishContent
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "cultures")]
    public string[]? Cultures { get; set; }
}
