using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
internal class DocumentUrlAliasDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrlAlias;

    [Column("id")]
    [PrimaryKeyColumn(Clustered = false, AutoIncrement = true)]
    public int Id { get; set; }

    // Unique index on (uniqueId, languageId, alias) - prevents duplicate entries
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "uniqueId, languageId, alias", Name = "IX_" + TableName + "_Unique")]
    [Column("uniqueId")]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    public Guid UniqueId { get; set; }

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
    [Column("languageId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(LanguageDto))]
    public int? NullableLanguageId { get; set; }

    // Lookup index on (alias, languageId) for fast retrieval
    [Index(IndexTypes.NonClustered, ForColumns = "alias, languageId", Name = "IX_" + TableName + "_Lookup")]
    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Alias { get; set; } = string.Empty;
}
