using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing the navigation ("apps") inside an editor in the back office
/// </summary>
[DataContract(Name = "user", Namespace = "")]
public class EditorNavigation
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "alias")]
    public string? Alias { get; set; }

    [DataMember(Name = "icon")]
    public string? Icon { get; set; }

    [DataMember(Name = "view")]
    public string? View { get; set; }

    [DataMember(Name = "active")]
    public bool Active { get; set; }
}
