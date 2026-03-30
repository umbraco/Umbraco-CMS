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

    /// <summary>
    /// Gets or sets the content type identifier associated with this cleanup policy.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoContentVersionCleanupPolicy")]
    [ForeignKey(typeof(ContentTypeDto), Column = ContentTypeDto.NodeIdColumnName)]
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether cleanup of content versions is prevented.
    /// </summary>
    [Column("preventCleanup")]
    public bool PreventCleanup { get; set; }

    /// <summary>
    /// Gets or sets the number of days; all content versions newer than this value (in days) will be retained during cleanup.
    /// </summary>
    [Column("keepAllVersionsNewerThanDays")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? KeepAllVersionsNewerThanDays { get; set; }

    /// <summary>
    /// Gets or sets the number of days for which the latest content version per day is retained.
    /// </summary>
    [Column("keepLatestVersionPerDayForDays")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? KeepLatestVersionPerDayForDays { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the content version cleanup policy was last updated.
    /// </summary>
    [Column("updated")]
    public DateTime Updated { get; set; }
}
