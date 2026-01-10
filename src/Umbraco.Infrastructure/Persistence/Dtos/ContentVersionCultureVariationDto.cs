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

    private const string LanguageIdName = "languageId";
    private const string VersionIdName = "versionId";
    private const string UpdateUserIdName = "availableUserId";
    private const string UpdateDateName = "date";
    private const string NameName = "name";
    private int? _updateUserId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(VersionIdName)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = $"{VersionIdName},{LanguageIdName}", IncludeColumns = $"{PrimaryKeyColumnName},{NameName},{UpdateDateName},{UpdateUserIdName}")]
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
