using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DictionaryDtoConfiguration))]
public class DictionaryDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryEntry;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string UniqueIdColumnName = "id";

    /// <summary>
    ///     Gets or sets the primary key of the dictionary entry.
    /// </summary>
    public int PrimaryKey { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier for the dictionary item.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the parent dictionary item, or <c>null</c> if this item has no parent.
    /// </summary>
    public Guid? Parent { get; set; }

    /// <summary>
    ///     Gets or sets the key for the dictionary item.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the collection of language text entries associated with this dictionary entry.
    /// </summary>
    public List<LanguageTextDto> LanguageTextDtos { get; set; } = [];

    /// <summary>
    ///     Gets or sets the parent dictionary entry navigation property.
    /// </summary>
    public DictionaryDto? ParentEntry { get; set; }
}
