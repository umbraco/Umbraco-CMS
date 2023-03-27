using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Used to publish content and variants
/// </summary>
[DataContract(Name = "publish", Namespace = "")]
public class PublishContent
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "cultures")]
    public string[]? Cultures { get; set; }
}
