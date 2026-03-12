using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class MediaVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.MediaVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string PathColumnName = "path";

    /// <summary>
    /// Gets or sets the primary key identifier for this media version record.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName, ForColumns = $"{PrimaryKeyColumnName}, {PathColumnName}")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the hierarchical path of the media version within the media tree.
    /// </summary>
    [Column(PathColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the content version DTO related to this media version.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
