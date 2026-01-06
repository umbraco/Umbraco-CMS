using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentTypeTemplateDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentType;
    public const string PrimaryKeyName = "contentTypeNodeId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsDocumentType", OnColumns = "contentTypeNodeId, templateNodeId")]
    [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
    [ForeignKey(typeof(NodeDto))]
    public int ContentTypeNodeId { get; set; }

    [Column("templateNodeId")]
    [ForeignKey(typeof(TemplateDto), Column = "nodeId")]
    public int TemplateNodeId { get; set; }

    [Column("IsDefault")]
    [Constraint(Default = "0")]
    public bool IsDefault { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
