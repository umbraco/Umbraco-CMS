using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Relation)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class RelationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Relation;

    [Column("id")]
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
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoRelation_relType", ForColumns = "relType,id", IncludeColumns ="parentId,childId,dateTime,comment")]
    public int RelationType { get; set; }

    [Column("datetime")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
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
