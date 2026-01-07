using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName)]
[ExplicitColumns]
internal sealed class RelationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Relation;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("parentId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelation_parentChildType", ForColumns = "parentId,childId,relType")]
    public int ParentId { get; set; }

    [Column("childId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode1")]
    public int ChildId { get; set; }

    [Column("relType")]
    [ForeignKey(typeof(RelationTypeDto))]
    public int RelationType { get; set; }

    [Column("datetime")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime Datetime { get; set; }

    [Column("comment")]
    [Length(1000)]
    public string? Comment { get; set; }

    [ResultColumn]
    [Column("parentObjectType")]
    public Guid ParentObjectType { get; set; }

    [ResultColumn]
    [Column("childObjectType")]
    public Guid ChildObjectType { get; set; }
}
