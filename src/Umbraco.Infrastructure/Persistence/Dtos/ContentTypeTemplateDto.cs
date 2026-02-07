using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([ContentTypeNodeIdColumnName, TemplateNodeIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentTypeTemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentType;
    public const string PrimaryKeyColumnName = "PK_cmsDocumentType";
    public const string ContentTypeNodeIdColumnName = "contentTypeNodeId";
    public const string TemplateNodeIdColumnName = "templateNodeId";

    [Column(ContentTypeNodeIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = PrimaryKeyColumnName, OnColumns = $"{ContentTypeNodeIdColumnName}, {TemplateNodeIdColumnName}")]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    public int ContentTypeNodeId { get; set; }

    [Column(TemplateNodeIdColumnName)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdColumnName)]
    public int TemplateNodeId { get; set; }

    [Column("IsDefault")]
    [Constraint(Default = "0")]
    public bool IsDefault { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
