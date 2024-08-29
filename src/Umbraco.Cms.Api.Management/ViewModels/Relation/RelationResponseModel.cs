using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Relation;

public class RelationResponseModel
{
    public RelationResponseModel(ReferenceByIdModel relationType, RelationReferenceModel parent, RelationReferenceModel child)
    {
        RelationType = relationType;
        Parent = parent;
        Child = child;
    }

    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the Type of relation
    /// </summary>
    public ReferenceByIdModel RelationType { get; set; }

    /// <summary>
    ///     Gets or sets the Parent of the Relation (Source).
    /// </summary>
    [ReadOnly(true)]
    [Required]
    public RelationReferenceModel Parent { get; set; }

    /// <summary>
    ///     Gets or sets the Child of the Relation (Destination).
    /// </summary>
    [ReadOnly(true)]
    [Required]
    public RelationReferenceModel Child { get; set; }

    /// <summary>
    ///     Gets or sets the date when the Relation was created.
    /// </summary>
    [ReadOnly(true)]
    public DateTimeOffset CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets a comment for the Relation.
    /// </summary>
    [ReadOnly(true)]
    public string? Comment { get; set; }
}
