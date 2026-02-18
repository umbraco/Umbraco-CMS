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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
    public int Id { get; set; }

    [Column("typeUniqueId")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_UniqueId")]
    public Guid UniqueId { get; set; }

    [Column("dual")]
    public bool Dual { get; set; }

    [Column("parentObjectType")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? ParentObjectType { get; set; }

    [Column("childObjectType")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? ChildObjectType { get; set; }

    [Column("name")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_name")]
    public string Name { get; set; } = null!;

    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Length(100)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_alias")]
    public string Alias { get; set; } = null!;

    [Constraint(Default = "0")]
    [Column("isDependency")]
    public bool IsDependency { get; set; }
}
