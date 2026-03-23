using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class AccessDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Access;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the access record.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoAccess", AutoIncrement = false)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the node identifier associated with this access entry.
    /// </summary>
    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoAccess_nodeId")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the node associated with login access.
    /// </summary>
    [Column("loginNodeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id1")]
    public int LoginNodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the node to which access is denied.
    /// </summary>
    [Column("noAccessNodeId")]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id2")]
    public int NoAccessNodeId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access record was created.
    /// </summary>
    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access record was last updated.
    /// </summary>
    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the access rules associated with this access entry.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(AccessRuleDto.AccessId))]
    public List<AccessRuleDto> Rules { get; set; } = new();
}
