using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DocumentUrlDtoConfiguration))]
public class DocumentUrlDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrl;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string UniqueIdColumnName = "uniqueId";
    public const string IsDraftColumnName = "isDraft";
    public const string LanguageIdColumnName = "languageId";
    public const string UrlSegmentColumnName = "urlSegment";
    public const string IsPrimaryColumnName = "isPrimary";

    /// <summary>
    /// Gets or sets the surrogate primary key identifier.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the document node associated with this URL.
    /// </summary>
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document URL is a draft.
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Gets or sets the language Id. NULL indicates invariant content (not language-specific).
    /// </summary>
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the URL segment, which is the part of the document's URL typically used for routing.
    /// </summary>
    public string UrlSegment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this URL is the primary URL.
    /// </summary>
    public bool IsPrimary { get; set; }
}
