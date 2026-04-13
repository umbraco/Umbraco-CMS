using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data transfer object representing a specific version of a document in Umbraco CMS, typically used for persistence operations.
/// </summary>
[TableName(TableName)]
[PrimaryKey(IContentVersionDto.Columns.Id, AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;

    private const string TemplateIdColumnName = "templateId";

    /// <summary>
    /// Gets or sets the unique identifier for the document version.
    /// </summary>
    [Column(IContentVersionDto.Columns.Id)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{IContentVersionDto.Columns.Id},{IContentVersionDto.Columns.Published}", IncludeColumns = TemplateIdColumnName)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the template identifier associated with the document version.
    /// </summary>
    [Column(TemplateIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdColumnName)]
    public int? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document version is published.
    /// </summary>
    [Column(IContentVersionDto.Columns.Published)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = IContentVersionDto.Columns.Published, IncludeColumns = $"{IContentVersionDto.Columns.Id},{TemplateIdColumnName}")]
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentVersionDto"/> associated with this document version.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
