
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

    /// <summary>
    /// Gets or sets the language Id.
    /// </summary>
    /// <remarks>
    /// This property returns 0 for invariant content. Use <see cref="NullableLanguageId"/> instead,
    /// which correctly returns <c>null</c> for invariant content.
    /// </remarks>
    [Obsolete("Use NullableLanguageId instead. This property returns 0 for invariant content. Scheduled for removal in Umbraco 18, when the NullableLanguageId will also be renamed to LanguageId.")]
    [Ignore]
    public int LanguageId
    {
        get => NullableLanguageId ?? 0;
        set => NullableLanguageId = value;
    }

    /// <summary>
    /// Gets or sets the language Id. NULL indicates invariant content (not language-specific).
    /// </summary>
    [Column(LanguageIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(LanguageDto))]
    public int? NullableLanguageId { get; set; }

    [Column(UrlSegmentColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string UrlSegment { get; set; } = string.Empty;

    [Column(IsPrimaryColumnName)]
    [Constraint(Default = 1)]
    public bool IsPrimary { get; set; }
}
