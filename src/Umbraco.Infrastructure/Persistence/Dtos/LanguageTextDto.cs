using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object for a language text entry, typically used for localization or translation in the system.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class LanguageTextDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryValue;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;

    private const string LanguageIdColumnName = "languageId";
    private const string UniqueIdColumnName = "UniqueId";

    /// <summary>
    /// Gets or sets the unique identifier (primary key) for the language text entry.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated language.
    /// </summary>
    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto), Column = LanguageDto.PrimaryKeyColumnName)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_languageId", ForColumns = $"{LanguageIdColumnName},{UniqueIdColumnName}")]
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) associated with this language text entry.
    /// </summary>
    [Column(UniqueIdColumnName)]
    [ForeignKey(typeof(DictionaryDto), Column = DictionaryDto.UniqueIdColumnName)]
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the localized text value associated with the language entry.
    /// </summary>
    [Column("value")]
    [Length(1000)]
    public string Value { get; set; } = null!;
}
