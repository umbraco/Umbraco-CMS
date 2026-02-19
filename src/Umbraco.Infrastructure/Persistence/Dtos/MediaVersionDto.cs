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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName, ForColumns = $"{PrimaryKeyColumnName}, {PathColumnName}")]
    public int Id { get; set; }

    [Column(PathColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Path { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
