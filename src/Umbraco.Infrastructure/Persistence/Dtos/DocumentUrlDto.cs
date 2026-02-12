
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class DocumentUrlDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrl;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string UniqueIdColumnName = "uniqueId";
    public const string IsDraftColumnName = "isDraft";
    public const string LanguageIdColumnName = "languageId";
    public const string UrlSegmentColumnName = "urlSegment";
    public const string IsPrimaryColumnName = "isPrimary";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Clustered = false, AutoIncrement = true)]
    public int NodeId { get; set; }

    [Index(IndexTypes.UniqueClustered, ForColumns = $"{UniqueIdColumnName}, {LanguageIdColumnName}, {IsDraftColumnName}, {UrlSegmentColumnName}", Name = "IX_" + TableName)]
    [Column(UniqueIdColumnName)]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    public Guid UniqueId { get; set; }

    [Column(IsDraftColumnName)]
    public bool IsDraft { get; set; }

    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    public int LanguageId { get; set; }

    [Column(UrlSegmentColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string UrlSegment { get; set; } = string.Empty;

    [Column(IsPrimaryColumnName)]
    [Constraint(Default = 1)]
    public bool IsPrimary { get; set; }
}
