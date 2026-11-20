using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// A row of the media version table, which extends a content version with the path of the file it stores.
/// </summary>
[EntityTypeConfiguration(typeof(MediaVersionDtoConfiguration))]
public class MediaVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.MediaVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PathColumnName = "path";

    /// <summary>
    /// Gets or sets the version identifier, shared with the content version row.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the path of the file this media version stores, or <c>null</c> when it stores no file.
    /// </summary>
    public string? Path { get; set; }
}
