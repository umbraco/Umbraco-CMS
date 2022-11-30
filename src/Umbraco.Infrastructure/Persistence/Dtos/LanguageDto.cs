using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class LanguageDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Language;

    /// <summary>
    ///     Gets or sets the identifier of the language.
    /// </summary>
    [Column("id")]
    [PrimaryKeyColumn(IdentitySeed = 2)]
    public short Id { get; set; }

    /// <summary>
    ///     Gets or sets the ISO code of the language.
    /// </summary>
    [Column("languageISOCode")]
    [Index(IndexTypes.UniqueNonClustered)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(14)]
    public string? IsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the culture name of the language.
    /// </summary>
    [Column("languageCultureName")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(100)]
    public string? CultureName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the language is the default language.
    /// </summary>
    [Column("isDefaultVariantLang")]
    [Constraint(Default = "0")]
    public bool IsDefault { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the language is mandatory.
    /// </summary>
    [Column("mandatory")]
    [Constraint(Default = "0")]
    public bool IsMandatory { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of a fallback language.
    /// </summary>
    [Column("fallbackLanguageId")]
    [ForeignKey(typeof(LanguageDto), Column = "id")]
    [Index(IndexTypes.NonClustered)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? FallbackLanguageId { get; set; }
}
