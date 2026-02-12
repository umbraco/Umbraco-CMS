using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class ContentVersionCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string LanguageIdColumnName = "languageId";
    private const string VersionIdColumnName = "versionId";
    private const string UpdateUserIdColumnName = "availableUserId";
    private const string UpdateDateColumnName = "date";
    private const string NameColumnName = "name";

    private int? _updateUserId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(VersionIdColumnName)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = $"{VersionIdColumnName},{LanguageIdColumnName}", IncludeColumns = $"{PrimaryKeyColumnName},{NameColumnName},{UpdateDateColumnName},{UpdateUserIdColumnName}")]
    public int VersionId { get; set; }

    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    // this is convenient to carry the culture around, but has no db counterpart
    [Ignore]
    public string? Culture { get; set; }

    [Column(NameColumnName)]
    public string? Name { get; set; }

    [Column(UpdateDateColumnName)] // TODO: db rename to 'updateDate'
    public DateTime UpdateDate { get; set; }

    [Column(UpdateUserIdColumnName)] // TODO: db rename to 'updateDate'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UpdateUserId
    {
        get => _updateUserId == 0 ? null : _updateUserId;
        set => _updateUserId = value;
    } // return null if zero
}
