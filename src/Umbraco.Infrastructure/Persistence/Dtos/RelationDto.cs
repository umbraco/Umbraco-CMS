using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class RelationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Relation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string RelationTypeColumnName = "relType";
    private const string ParentIdColumnName = "parentId";
    private const string ChildIdColumnName = "childId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(ParentIdColumnName)]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelation_parentChildType", ForColumns = $"{ParentIdColumnName},{ChildIdColumnName},{RelationTypeColumnName}")]
    public int ParentId { get; set; }

    [Column(ChildIdColumnName)]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode1")]
    public int ChildId { get; set; }

    [Column(RelationTypeColumnName)]
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
