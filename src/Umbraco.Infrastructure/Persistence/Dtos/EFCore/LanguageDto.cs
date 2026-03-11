using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(LanguageDtoConfiguration))]
public class LanguageDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Language;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    // Public constants to bind properties between DTOs
    public const string IsoCodeColumnName = "languageISOCode";

    /// <summary>
    ///     Gets or sets the identifier of the language.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the ISO code of the language.
    /// </summary>
    public string? IsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the culture name of the language.
    /// </summary>
    public string? CultureName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the language is the default language.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the language is mandatory.
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of a fallback language.
    /// </summary>
    public int? FallbackLanguageId { get; set; }

    /// <summary>
    ///     Gets or sets the fallback language navigation property.
    /// </summary>
    public LanguageDto? FallbackLanguage { get; set; }
}
