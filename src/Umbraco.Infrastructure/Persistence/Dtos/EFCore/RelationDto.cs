using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// Data transfer object representing a relation between two nodes in the Umbraco CMS persistence layer.
/// </summary>
[EntityTypeConfiguration(typeof(RelationDtoConfiguration))]
public class RelationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Relation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    // Public constants to bind properties between configurations and consumers.
    public const string IdColumnName = PrimaryKeyColumnName;
    public const string ParentIdColumnName = "parentId";
    public const string ChildIdColumnName = "childId";
    public const string RelationTypeColumnName = "relType";
    public const string DatetimeColumnName = "datetime";
    public const string CommentColumnName = "comment";

    /// <summary>
    /// Gets or sets the unique identifier for the relation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent node in this relation.
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the child node in this relation.
    /// </summary>
    public int ChildId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the relation type associated with this relation.
    /// </summary>
    public int RelationType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the relation was created.
    /// </summary>
    public DateTime Datetime { get; set; }

    /// <summary>
    /// Gets or sets the comment associated with the relation.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the parent node referenced by <see cref="ParentId"/>.
    /// </summary>
    public NodeDto? ParentNode { get; set; }

    /// <summary>
    /// Gets or sets the child node referenced by <see cref="ChildId"/>.
    /// </summary>
    public NodeDto? ChildNode { get; set; }

    /// <summary>
    /// Gets or sets the relation type referenced by <see cref="RelationType"/>.
    /// </summary>
    public RelationTypeDto? RelationTypeDto { get; set; }
}
