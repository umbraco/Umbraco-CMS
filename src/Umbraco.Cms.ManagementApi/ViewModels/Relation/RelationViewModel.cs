using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Relation;

public class RelationViewModel
{
    /// <summary>
    ///     Gets or sets the Parent Id of the Relation (Source).
    /// </summary>
    [JsonPropertyName("parentId")]
    [ReadOnly(true)]
    public int ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the Parent Name of the relation (Source).
    /// </summary>
    [JsonPropertyName("parentName")]
    [ReadOnly(true)]
    public string? ParentName { get; set; }

    /// <summary>
    ///     Gets or sets the Child Id of the Relation (Destination).
    /// </summary>
    [JsonPropertyName("childId")]
    [ReadOnly(true)]
    public int ChildId { get; set; }

    /// <summary>
    ///     Gets or sets the Child Name of the relation (Destination).
    /// </summary>
    [JsonPropertyName("childName")]
    [ReadOnly(true)]
    public string? ChildName { get; set; }

    /// <summary>
    ///     Gets or sets the date when the Relation was created.
    /// </summary>
    [JsonPropertyName("createDate")]
    [ReadOnly(true)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets a comment for the Relation.
    /// </summary>
    [JsonPropertyName("comment")]
    [ReadOnly(true)]
    public string? Comment { get; set; }
}
