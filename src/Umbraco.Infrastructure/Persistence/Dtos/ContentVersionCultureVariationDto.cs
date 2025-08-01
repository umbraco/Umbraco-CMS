using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id")]
[ExplicitColumns]
internal sealed class ContentVersionCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;
    private int? _updateUserId;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("versionId")]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = "versionId,languageId", IncludeColumns = "id,name,date,availableUserId")]
    public int VersionId { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    // this is convenient to carry the culture around, but has no db counterpart
    [Ignore]
    public string? Culture { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("date", ForceToUtc = false)] // TODO: db rename to 'updateDate'
    public DateTime UpdateDate { get; set; }

    [Column("availableUserId")] // TODO: db rename to 'updateDate'
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? UpdateUserId
    {
        get => _updateUserId == 0 ? null : _updateUserId;
        set => _updateUserId = value;
    } // return null if zero
}
