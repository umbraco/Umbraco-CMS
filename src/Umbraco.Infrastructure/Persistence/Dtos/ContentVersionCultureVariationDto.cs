using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName)]
[ExplicitColumns]
internal sealed class ContentVersionCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;
    public const string PrimaryKeyName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string LanguageIdName = "languageId";
    public const string VersionIdName = "versionId";
    public const string UpdateUserIdName = "availableUserId";
    public const string UpdateDateName = "date";
    public const string NameName = "name";
    private int? _updateUserId;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(VersionIdName)]
    [ForeignKey(typeof(ContentVersionDto), Name = "FK_umbContentVersionCultureVariation_umbContentVersion_id")] // needs to be shorter than 64 chars for e.g. PostgreSQL!
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = $"{VersionIdName},{LanguageIdName}", IncludeColumns = $"{PrimaryKeyName},{NameName},{UpdateDateName},{UpdateUserIdName}")]
    public int VersionId { get; set; }

    [Column(LanguageIdName)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    // this is convenient to carry the culture around, but has no db counterpart
    [Ignore]
    public string? Culture { get; set; }

    [Column(NameName)]
    public string? Name { get; set; }

    [Column(UpdateDateName)] // TODO: db rename to 'updateDate'
    public DateTime UpdateDate { get; set; }

    [Column(UpdateUserIdName)] // TODO: db rename to 'updateDate'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UpdateUserId
    {
        get => _updateUserId == 0 ? null : _updateUserId;
        set => _updateUserId = value;
    } // return null if zero
}
