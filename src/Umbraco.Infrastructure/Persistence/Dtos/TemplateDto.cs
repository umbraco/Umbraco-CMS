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

    /// <summary>
    /// Gets or sets the primary key of the template.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the node associated with this template.
    /// </summary>
    [Column(NodeIdColumnName)]
    [Index(IndexTypes.UniqueNonClustered)]
    [ForeignKey(typeof(NodeDto), Name = "FK_cmsTemplate_umbracoNode")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique alias (identifier) of the template.
    /// </summary>
    [Column("alias")]
    [Length(100)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the NodeDto entity associated with this template, representing the related node in the database.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(NodeId))]
    public NodeDto NodeDto { get; set; } = null!;
}
