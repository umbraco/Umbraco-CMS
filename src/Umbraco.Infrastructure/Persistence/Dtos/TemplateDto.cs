using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName)]
[ExplicitColumns]
internal sealed class TemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Template;
    public const string PrimaryKeyName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ReferenceName = "NodeId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(NodeIdName)]
    [Index(IndexTypes.UniqueNonClustered)]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsTemplate_umbracoNode")]
    public int NodeId { get; set; }

    [Column("alias")]
    [Length(100)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Alias { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceName)]
    public NodeDto NodeDto { get; set; } = null!;
}
