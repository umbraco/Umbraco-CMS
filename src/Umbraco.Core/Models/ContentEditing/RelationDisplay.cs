using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "relation", Namespace = "")]
public class RelationDisplay
{
    /// <summary>
    ///     Gets or sets the Parent Id of the Relation (Source).
    /// </summary>
    [DataMember(Name = "parentId")]
    [ReadOnly(true)]
    public int ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the Parent Name of the relation (Source).
    /// </summary>
    [DataMember(Name = "parentName")]
    [ReadOnly(true)]
    public string? ParentName { get; set; }

    /// <summary>
    ///     Gets or sets the Child Id of the Relation (Destination).
    /// </summary>
    [DataMember(Name = "childId")]
    [ReadOnly(true)]
    public int ChildId { get; set; }

    /// <summary>
    ///     Gets or sets the Child Name of the relation (Destination).
    /// </summary>
    [DataMember(Name = "childName")]
    [ReadOnly(true)]
    public string? ChildName { get; set; }

    /// <summary>
    ///     Gets or sets the date when the Relation was created.
    /// </summary>
    [DataMember(Name = "createDate")]
    [ReadOnly(true)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets a comment for the Relation.
    /// </summary>
    [DataMember(Name = "comment")]
    [ReadOnly(true)]
    public string? Comment { get; set; }
}
