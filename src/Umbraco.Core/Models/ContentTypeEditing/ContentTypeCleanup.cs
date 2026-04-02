namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the cleanup settings for content version history of a content type.
/// </summary>
public class ContentTypeCleanup
{
    /// <summary>
    ///     Gets or sets a value indicating whether automatic cleanup of content versions is prevented.
    /// </summary>
    public bool PreventCleanup { get; init; }

    /// <summary>
    ///     Gets or sets the number of days for which all content versions should be retained.
    /// </summary>
    public int? KeepAllVersionsNewerThanDays { get; init; }

    /// <summary>
    ///     Gets or sets the number of days for which to keep only the latest version per day.
    /// </summary>
    public int? KeepLatestVersionPerDayForDays { get; init; }
}
