using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ContentVersionCleanupPolicyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCleanupPolicy;
    public const string PrimaryKeyColumnName = "contentTypeId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = $"PK_{TableName}")]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeId { get; set; }

    [Column("preventCleanup")]
    public bool PreventCleanup { get; set; }

    [Column("keepAllVersionsNewerThanDays")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? KeepAllVersionsNewerThanDays { get; set; }

    [Column("keepLatestVersionPerDayForDays")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? KeepLatestVersionPerDayForDays { get; set; }

    [Column("updated")]
    public DateTime Updated { get; set; }
}
