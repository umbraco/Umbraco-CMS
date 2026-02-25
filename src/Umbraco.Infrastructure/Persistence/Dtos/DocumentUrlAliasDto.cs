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

    /// <summary>
    /// Gets or sets the unique identifier for the document URL alias.
    /// </summary>
    [Column("id")]
    [PrimaryKeyColumn(Clustered = false, AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the document associated with this URL alias.
    /// </summary>
    /// <remarks>Unique index on (uniqueId, languageId, alias) - prevents duplicate entries</remarks>
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "uniqueId, languageId, alias", Name = "IX_" + TableName + "_Unique")]
    [Column("uniqueId")]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with this document URL alias.
    /// </summary>
    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the alias string that represents an alternative URL for the document.
    /// Used for URL aliasing and fast retrieval in combination with language ID.
    /// </summary>
    /// <remarks>Lookup index on (alias, languageId) for fast retrieval</remarks>
    [Index(IndexTypes.NonClustered, ForColumns = "alias, languageId", Name = "IX_" + TableName + "_Lookup")]
    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Alias { get; set; } = string.Empty;
}
