using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DocumentUrlAliasDtoConfiguration))]
public class DocumentUrlAliasDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrlAlias;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string UniqueIdColumnName = "uniqueId";
    public const string LanguageIdColumnName = "languageId";
    public const string AliasColumnName = "alias";

    /// <summary>
    /// Gets or sets the unique identifier for the document URL alias.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the document associated with this URL alias.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with this document URL alias.
    /// </summary>
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the alias string that represents an alternative URL for the document.
    /// </summary>
    public string Alias { get; set; } = string.Empty;
}
