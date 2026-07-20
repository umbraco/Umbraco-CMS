using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Relation;

/// <summary>
/// Represents a response model containing information about a relation returned by the Umbraco CMS Management API.
/// </summary>
public class RelationResponseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.ViewModels.Relation.RelationResponseModel"/> class.
    /// </summary>
    /// <param name="relationType">The relation type, referenced by its identifier.</param>
    /// <param name="parent">The parent entity in the relation.</param>
    /// <param name="child">The child entity in the relation.</param>
    public RelationResponseModel(ReferenceByIdModel relationType, RelationReferenceModel parent, RelationReferenceModel child)
    {
        RelationType = relationType;
        Parent = parent;
        Child = child;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the relation.
    /// </summary>
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
