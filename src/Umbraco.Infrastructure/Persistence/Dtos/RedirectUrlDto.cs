using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class RedirectUrlDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.RedirectUrl;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string ContentKeyColumnName = "contentKey";
    private const string CreateDateUtcColumnName = "createDateUtc";
    private const string UrlColumnName = "url";
    private const string CultureColumnName = "culture";
    private const string UrlHashColumnName = "urlHash";

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlDto"/> class with default values.
    /// </summary>
    public RedirectUrlDto() => CreateDateUtc = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier for the redirect URL.
    /// </summary>
    /// <remarks>
    /// notes
    /// we want a unique, non-clustered  index on (url ASC, contentId ASC, culture ASC, createDate DESC) but the
    /// problem is that the index key must be 900 bytes max. should we run without an index? done
    /// some perfs comparisons, and running with an index on a hash is only slightly slower on
    /// inserts, and much faster on reads, so... we have an index on a hash.
    /// </remarks>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoRedirectUrl", AutoIncrement = false)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content item to which the redirect URL points.
    /// </summary>
    [ResultColumn]
    public int ContentId { get; set; }

    /// <summary>
    /// Gets or sets the content key associated with the redirect URL.
    /// </summary>
    [Column(ContentKeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    public Guid ContentKey { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the redirect URL was created.
    /// </summary>
    [Column(CreateDateUtcColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_culture_hash", ForColumns = CreateDateUtcColumnName, IncludeColumns = $"{CultureColumnName},{UrlColumnName},{UrlHashColumnName},{ContentKeyColumnName}")]
    public DateTime CreateDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the destination URL to which requests are redirected.
    /// </summary>
    [Column(UrlColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the culture associated with the redirect URL.
    /// </summary>
    [Column(CultureColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the hash representation of the URL for uniqueness and indexing purposes.
    /// </summary>
    [Column(UrlHashColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRedirectUrl", ForColumns = $"{UrlHashColumnName}, {ContentKeyColumnName}, {CultureColumnName}, {CreateDateUtcColumnName}")]
    [Length(40)]
    public string UrlHash { get; set; } = null!;
}
