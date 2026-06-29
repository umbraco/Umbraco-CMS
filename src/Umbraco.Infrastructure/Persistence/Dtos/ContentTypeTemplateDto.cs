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
    public const string TemplateNodeIdColumnName = "templateNodeId";
    public const string ContentTypeNodeIdColumnName = "contentTypeNodeId";

    /// <summary>
    /// Gets or sets the node ID of the associated content type.
    /// </summary>
    [Column(ContentTypeNodeIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsDocumentType", OnColumns = $"{ContentTypeNodeIdColumnName}, {TemplateNodeIdColumnName}")]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    public int ContentTypeNodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated template node.
    /// </summary>
    [Column(TemplateNodeIdColumnName)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdColumnName)]
    public int TemplateNodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this template is set as the default template for the content type.
    /// </summary>
    [Column("IsDefault")]
    [Constraint(Default = "0")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentTypeDto"/> associated with this template mapping.
    /// This represents the content type to which the template is linked.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentTypeDto? ContentTypeDto { get; set; }
}
