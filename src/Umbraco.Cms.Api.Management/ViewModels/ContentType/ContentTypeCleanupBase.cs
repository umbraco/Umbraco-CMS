namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base class for view models related to content type cleanup operations in the management API.
/// </summary>
public abstract class ContentTypeCleanupBase
{
    /// <summary>
    /// Gets a value indicating whether cleanup should be prevented.
    /// </summary>
    public bool PreventCleanup { get; init; }

    /// <summary>
    /// Gets or sets the number of days; all content versions newer than this value will be retained.
    /// </summary>
    public int? KeepAllVersionsNewerThanDays { get; init; }

    /// <summary>
    /// Gets or sets the number of days to keep the latest version per day.
    /// </summary>
    public int? KeepLatestVersionPerDayForDays { get; init; }
}
