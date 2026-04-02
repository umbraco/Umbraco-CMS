using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a tag data transfer model for API serialization.
/// </summary>
[DataContract(Name = "tag", Namespace = "")]
public class TagModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the tag.
    /// </summary>
    [DataMember(Name = "id", IsRequired = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the text content of the tag.
    /// </summary>
    [DataMember(Name = "text", IsRequired = true)]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the group that the tag belongs to.
    /// </summary>
    [DataMember(Name = "group")]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the count of nodes that have this tag assigned.
    /// </summary>
    [DataMember(Name = "nodeCount")]
    public int NodeCount { get; set; }
}
