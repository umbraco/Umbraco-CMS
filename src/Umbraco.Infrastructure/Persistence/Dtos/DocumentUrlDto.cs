
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
public class DocumentUrlDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrl;

    [Column("id")]
    [PrimaryKeyColumn(Clustered = false, AutoIncrement = true)]
    public int NodeId { get; set; }

    [Index(IndexTypes.UniqueClustered, ForColumns = "uniqueId, languageId, isDraft, urlSegment", Name = "IX_" + TableName)]
    [Column("uniqueId")]
    [ForeignKey(typeof(NodeDto), Column = "uniqueId")]
    public Guid UniqueId { get; set; }

    [Column("isDraft")]
    public bool IsDraft { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    public int LanguageId { get; set; }

    [Column("urlSegment")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string UrlSegment { get; set; } = string.Empty;

}
