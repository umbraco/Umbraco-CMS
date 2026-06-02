using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// Data transfer object representing a relation type in the Umbraco CMS persistence layer.
/// </summary>
[EntityTypeConfiguration(typeof(RelationTypeDtoConfiguration))]
public class RelationTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.RelationType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public const int NodeIdSeed = 10;

    // Public constants to bind properties between configurations and consumers.
    public const string IdColumnName = PrimaryKeyColumnName;
    public const string UniqueIdColumnName = "typeUniqueId";
    public const string DualColumnName = "dual";
    public const string ParentObjectTypeColumnName = "parentObjectType";
    public const string ChildObjectTypeColumnName = "childObjectType";
    public const string NameColumnName = "name";
    public const string AliasColumnName = "alias";
    public const string IsDependencyColumnName = "isDependency";

    /// <summary>
    /// Gets or sets the unique identifier for the relation type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) for the relation type.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this relation type is dual, meaning it can be used in both directions between related entities.
    /// </summary>
    public bool Dual { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the parent object type in the relation.
    /// </summary>
    public Guid? ParentObjectType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the child object type.
    /// </summary>
    public Guid? ChildObjectType { get; set; }

    /// <summary>
    /// Gets or sets the name of the relation type.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique alias that identifies the relation type.
    /// </summary>
    public string Alias { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether this relation type represents a dependency between related entities.
    /// </summary>
    public bool IsDependency { get; set; }
}
