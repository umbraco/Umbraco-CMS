using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Model representing the global content version cleanup policy
/// </summary>
public class ContentVersionCleanupPolicySettings
{
    private const bool StaticEnableCleanup = false;
    private const int StaticKeepAllVersionsNewerThanDays = 7;
    private const int StaticKeepLatestVersionPerDayForDays = 90;
    private const int StaticMaxVersionsToDeletePerRun = 50_000;

    /// <summary>
    ///     Gets or sets a value indicating whether or not the cleanup job should be executed.
    /// </summary>
    [DefaultValue(StaticEnableCleanup)]
    public bool EnableCleanup { get; set; } = StaticEnableCleanup;

    /// <summary>
    ///     Gets or sets the number of days where all historical content versions are kept.
    /// </summary>
    [DefaultValue(StaticKeepAllVersionsNewerThanDays)]
    public int KeepAllVersionsNewerThanDays { get; set; } = StaticKeepAllVersionsNewerThanDays;

    /// <summary>
    ///     Gets or sets the number of days where the latest historical content version for that day are kept.
    /// </summary>
    [DefaultValue(StaticKeepLatestVersionPerDayForDays)]
    public int KeepLatestVersionPerDayForDays { get; set; } = StaticKeepLatestVersionPerDayForDays;

    /// <summary>
    ///     Gets or sets the maximum number of content versions to process per cleanup run.
    ///     When more versions are eligible, they will be processed in subsequent runs.
    ///     A value of 0 means no limit (process all eligible versions).
    /// </summary>
    [DefaultValue(StaticMaxVersionsToDeletePerRun)]
    public int MaxVersionsToDeletePerRun { get; set; } = StaticMaxVersionsToDeletePerRun;
}
