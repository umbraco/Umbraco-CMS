using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data transfer object representing a specific version of an element in Umbraco CMS, typically used for persistence operations.
/// </summary>
[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
public sealed class ElementVersionDto : IContentVersionDto
{
    /// <summary>
    /// The database table name for element versions.
    /// </summary>
    public const string TableName = Constants.DatabaseSchema.Tables.ElementVersion;

    /// <summary>
    /// Gets or sets the unique identifier for the element version.
    /// </summary>
    [Column(IContentVersionDto.Columns.Id)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{IContentVersionDto.Columns.Id},{IContentVersionDto.Columns.Published}")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this element version is published.
    /// </summary>
    [Column(IContentVersionDto.Columns.Published)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = IContentVersionDto.Columns.Published, IncludeColumns = IContentVersionDto.Columns.Id)]
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentVersionDto"/> associated with this element version.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
