using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
internal class ContentVersionCleanupPolicyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCleanupPolicy;

    [Column("contentTypeId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoContentVersionCleanupPolicy")]
    [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
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
