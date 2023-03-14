using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Used to create a folder with the MediaController
/// </summary>
[DataContract]
public class PostedFolder
{
    [DataMember(Name = "parentId")]
    public string? ParentId { get; set; }

    [DataMember(Name = "name")]
    public string? Name { get; set; }
}
