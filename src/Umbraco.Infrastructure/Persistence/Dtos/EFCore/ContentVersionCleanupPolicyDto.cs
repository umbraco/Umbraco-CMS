using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// Data transfer object for the per-content-type version cleanup policy.
/// </summary>
[EntityTypeConfiguration(typeof(ContentVersionCleanupPolicyDtoConfiguration))]
public class ContentVersionCleanupPolicyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCleanupPolicy;
    public const string PrimaryKeyColumnName = "contentTypeId";

    // Public constants to bind properties between configurations and consumers.
    public const string ContentTypeIdColumnName = PrimaryKeyColumnName;
    public const string PreventCleanupColumnName = "preventCleanup";
    public const string KeepAllVersionsNewerThanDaysColumnName = "keepAllVersionsNewerThanDays";
    public const string KeepLatestVersionPerDayForDaysColumnName = "keepLatestVersionPerDayForDays";
    public const string UpdatedColumnName = "updated";

    /// <summary>
    /// Gets or sets the content type identifier this policy applies to. Also the primary key.
    /// </summary>
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether cleanup of content versions is prevented for this content type.
    /// </summary>
    public bool PreventCleanup { get; set; }

    /// <summary>
    /// Gets or sets the number of days; all content versions newer than this value are retained during cleanup.
    /// </summary>
    public int? KeepAllVersionsNewerThanDays { get; set; }

    /// <summary>
    /// Gets or sets the number of days for which the latest content version per day is retained.
    /// </summary>
    public int? KeepLatestVersionPerDayForDays { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this policy was last updated.
    /// </summary>
    public DateTime Updated { get; set; }
}
