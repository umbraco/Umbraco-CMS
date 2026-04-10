using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentVersionCultureVariationDtoConfiguration))]
public class ContentVersionCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string VersionIdColumnName = "versionId";
    public const string LanguageIdColumnName = "languageId";
    public const string NameColumnName = "name";
    public const string UpdateDateColumnName = "date";
    public const string UpdateUserIdColumnName = "availableUserId";

    private int? _updateUserId;

    /// <summary>
    /// Gets or sets the unique identifier for the content version culture variation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated content version.
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the language associated with this content version culture variation.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the name associated with this culture variation of the content version.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the content version culture variation was last updated.
    /// </summary>
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this content version culture variation.
    /// </summary>
    /// <remarks>Returns null if zero.</remarks>
    public int? UpdateUserId
    {
        get => _updateUserId == 0 ? null : _updateUserId;
        set => _updateUserId = value;
    }
}
