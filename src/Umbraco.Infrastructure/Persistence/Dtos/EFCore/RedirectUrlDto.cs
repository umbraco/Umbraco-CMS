using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(RedirectUrlDtoConfiguration))]
public class RedirectUrlDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.RedirectUrl;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public const string ContentKeyColumnName = "contentKey";
    public const string CreateDateUtcColumnName = "createDateUtc";
    public const string UrlColumnName = "url";
    public const string CultureColumnName = "culture";
    public const string UrlHashColumnName = "urlHash";

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlDto"/> class with default values.
    /// </summary>
    public RedirectUrlDto() => CreateDateUtc = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier for the redirect URL.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the content key associated with the redirect URL.
    /// </summary>
    public Guid ContentKey { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the redirect URL was created.
    /// </summary>
    public DateTime CreateDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the destination URL to which requests are redirected.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the culture associated with the redirect URL.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the hash representation of the URL for uniqueness and indexing purposes.
    /// </summary>
    public string UrlHash { get; set; } = null!;
}
