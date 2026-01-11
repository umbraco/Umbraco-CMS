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

    private const string ContentKeyName = "contentKey";
    private const string CreateDateUtcName = "createDateUtc";
    private const string UrlName = "url";
    private const string CultureName = "culture";
    private const string UrlHashName = "urlHash";

    public RedirectUrlDto() => CreateDateUtc = DateTime.UtcNow;

    // notes
    //
    // we want a unique, non-clustered  index on (url ASC, contentId ASC, culture ASC, createDate DESC) but the
    // problem is that the index key must be 900 bytes max. should we run without an index? done
    // some perfs comparisons, and running with an index on a hash is only slightly slower on
    // inserts, and much faster on reads, so... we have an index on a hash.
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoRedirectUrl", AutoIncrement = false)]
    public Guid Id { get; set; }

    [ResultColumn]
    public int ContentId { get; set; }

    [Column(ContentKeyName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    public Guid ContentKey { get; set; }

    [Column(CreateDateUtcName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_culture_hash", ForColumns = CreateDateUtcName, IncludeColumns = $"{CultureName},{UrlName},{UrlHashName},{ContentKeyName}")]
    public DateTime CreateDateUtc { get; set; }

    [Column(UrlName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string Url { get; set; } = null!;

    [Column(CultureName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Culture { get; set; }

    [Column(UrlHashName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRedirectUrl", ForColumns = $"{UrlHashName}, {ContentKeyName}, {CultureName}, {CreateDateUtcName}")]
    [Length(40)]
    public string UrlHash { get; set; } = null!;
}
