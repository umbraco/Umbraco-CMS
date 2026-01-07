using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class AccessDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Access;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(Name = "PK_umbracoAccess", AutoIncrement = false)]
    public Guid Id { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoAccess_nodeId")]
    public int NodeId { get; set; }

    [Column("loginNodeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id1")]
    public int LoginNodeId { get; set; }

    [Column("noAccessNodeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id2")]
    public int NoAccessNodeId { get; set; }

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "AccessId")]
    public List<AccessRuleDto> Rules { get; set; } = new();
}
