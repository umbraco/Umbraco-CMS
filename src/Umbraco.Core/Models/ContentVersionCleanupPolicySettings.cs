namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the content version cleanup policy settings for a specific content type.
/// </summary>
/// <remarks>
///     These settings control how historical content versions are cleaned up to manage database size.
///     Each content type can have its own cleanup policy.
/// </remarks>
public class ContentVersionCleanupPolicySettings
{
    /// <summary>
    ///     Gets or sets the identifier of the content type these settings apply to.
    /// </summary>
    public int ContentTypeId { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether version cleanup should be prevented for this content type.
    /// </summary>
    /// <value>
    ///     <c>true</c> to prevent automatic cleanup of versions; <c>false</c> to allow cleanup.
    /// </value>
    public bool PreventCleanup { get; set; }

    /// <summary>
    ///     Gets or sets the number of days to keep all versions.
    /// </summary>
    /// <value>
    ///     The number of days during which all versions are retained, or <c>null</c> to use the global default.
    /// </value>
    /// <remarks>
    ///     All versions created within this time period will be kept regardless of other settings.
    /// </remarks>
    public int? KeepAllVersionsNewerThanDays { get; set; }

    /// <summary>
    ///     Gets or sets the number of days to keep the latest version per day.
    /// </summary>
    /// <value>
    ///     The number of days during which only the latest version per day is retained, or <c>null</c> to use the global default.
    /// </value>
    /// <remarks>
    ///     After the <see cref="KeepAllVersionsNewerThanDays" /> period, only one version per day is kept for this duration.
    /// </remarks>
    public int? KeepLatestVersionPerDayForDays { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when these settings were last updated.
    /// </summary>
    public DateTime Updated { get; set; }
}
