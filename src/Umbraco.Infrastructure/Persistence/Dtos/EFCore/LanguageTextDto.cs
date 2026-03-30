using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(LanguageTextDtoConfiguration))]
public class LanguageTextDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryValue;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;

    /// <summary>
    ///     Gets or sets the primary key of the language text entry.
    /// </summary>
    public int PrimaryKey { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the associated language.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier linking this translation to a dictionary entry.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    ///     Gets or sets the localized text value.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the dictionary entry navigation property.
    /// </summary>
    public DictionaryDto? DictionaryEntry { get; set; }

    /// <summary>
    ///     Gets or sets the language navigation property.
    /// </summary>
    public LanguageDto? Language { get; set; }
}
