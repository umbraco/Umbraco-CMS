using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for a content document within the Umbraco CMS persistence layer.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class DocumentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Document;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    // Public constants to bind properties between DTOs
    public const string PublishedColumnName = "published";

    /// <summary>
    /// Gets or sets the unique identifier for the node associated with this document.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is published.
    /// </summary>
    [Column(PublishedColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Published")]
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document has been modified since it was last published or saved.
    /// </summary>
    [Column("edited")]
    public bool Edited { get; set; }

    // [Column("publishDate")]
    // [NullSetting(NullSetting = NullSettings.Null)] // is contentVersionDto.VersionDate for the published version
    // public DateTime? PublishDate { get; set; }

    // [Column("publishUserId")]
    // [NullSetting(NullSetting = NullSettings.Null)] // is contentVersionDto.UserId for the published version
    // public int? PublishUserId { get; set; }

    // [Column("publishName")]
    // [NullSetting(NullSetting = NullSettings.Null)] // is contentVersionDto.Text for the published version
    // public string PublishName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentDto"/> instance that contains the content data associated with this document.
    /// </summary>
    /// <remarks>
    /// [Column("publishTemplateId")]
    /// [NullSetting(NullSetting = NullSettings.Null)] // is documentVersionDto.TemplateId for the published version
    /// public int? PublishTemplateId { get; set; }
    /// </remarks>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = nameof(ContentDto.NodeId))]
    public ContentDto ContentDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the document version data transfer object (DTO) that is referenced in a one-to-one relationship with this document.
    /// Although a document can have multiple versions, this property represents the specific version associated with this instance.
    /// </summary>
    /// <remarks>
    /// although a content has many content versions,
    /// they can only be loaded one by one (as several content),
    /// so this here is a OneToOne reference
    /// </remarks>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public DocumentVersionDto DocumentVersionDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DTO containing information about the published version of the document.
    /// </summary>
    /// <remarks>same</remarks>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public DocumentVersionDto? PublishedVersionDto { get; set; }
}
