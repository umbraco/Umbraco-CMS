using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class TemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Template;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(NodeIdColumnName)]
    [Index(IndexTypes.UniqueNonClustered)]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsTemplate_umbracoNode")]
    public int NodeId { get; set; }

    [Column("alias")]
    [Length(100)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Alias { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(NodeId))]
    public NodeDto NodeDto { get; set; } = null!;
}
