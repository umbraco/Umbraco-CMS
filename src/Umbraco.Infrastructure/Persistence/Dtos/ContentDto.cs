using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for content entities in Umbraco CMS, used for database persistence operations.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class ContentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Content;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ContentTypeIdColumnName = "contentTypeId";

    /// <summary>
    /// Gets or sets the identifier of the node.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type.
    /// </summary>
    [Column(ContentTypeIdColumnName)]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.ContentDto.NodeDto"/> representing the associated node entity for this content.
    /// This property establishes a one-to-one relationship between the content and its underlying node data.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = PrimaryKeyColumnName)]
    public NodeDto NodeDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content version data transfer object for this content.
    /// Represents a one-to-one relationship to the version information of the content item.
    /// </summary>
    /// <remarks>
    /// although a content has many content versions,
    /// they can only be loaded one by one (as several content),
    /// so this here is a OneToOne reference
    /// </remarks>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = nameof(ContentVersionDto.NodeId))]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
