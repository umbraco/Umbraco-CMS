using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class RelationTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.RelationType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public const int NodeIdSeed = 10;

    /// <summary>
    /// Gets or sets the unique identifier for the relation type.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the relation type.
    /// </summary>
    [Column("typeUniqueId")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_UniqueId")]
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this relation type is dual, meaning it can be used in both directions between related entities.
    /// </summary>
    [Column("dual")]
    public bool Dual { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the parent object type in the relation.
    /// </summary>
    [Column("parentObjectType")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? ParentObjectType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the child object type.
    /// </summary>
    [Column("childObjectType")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? ChildObjectType { get; set; }

    /// <summary>
    /// Gets or sets the name of the relation type.
    /// </summary>
    [Column("name")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique alias that identifies the relation type.
    /// </summary>
    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Length(100)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_alias")]
    public string Alias { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether this relation type represents a dependency between related entities.
    /// When set to <c>true</c>, the relation type is considered a dependency.
    /// </summary>
    [Constraint(Default = "0")]
    [Column("isDependency")]
    public bool IsDependency { get; set; }
}
