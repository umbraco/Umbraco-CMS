using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the history cleanup settings for content versions.
/// </summary>
[DataContract(Name = "historyCleanup", Namespace = "")]
public class HistoryCleanup : BeingDirtyBase
{
    private int? _keepAllVersionsNewerThanDays;
    private int? _keepLatestVersionPerDayForDays;
    private bool _preventCleanup;

    /// <summary>
    ///     Gets or sets a value indicating whether to prevent automatic history cleanup for this content.
    /// </summary>
    [DataMember(Name = "preventCleanup")]
    public bool PreventCleanup
    {
        get => _preventCleanup;
        set => SetPropertyValueAndDetectChanges(value, ref _preventCleanup, nameof(PreventCleanup));
    }

    /// <summary>
    ///     Gets or sets the number of days to keep all versions of this content.
    /// </summary>
    [DataMember(Name = "keepAllVersionsNewerThanDays")]
    public int? KeepAllVersionsNewerThanDays
    {
        get => _keepAllVersionsNewerThanDays;
        set => SetPropertyValueAndDetectChanges(value, ref _keepAllVersionsNewerThanDays, nameof(KeepAllVersionsNewerThanDays));
    }

    /// <summary>
    ///     Gets or sets the number of days to keep the latest version per day for this content.
    /// </summary>
    [DataMember(Name = "keepLatestVersionPerDayForDays")]
    public int? KeepLatestVersionPerDayForDays
    {
        get => _keepLatestVersionPerDayForDays;
        set => SetPropertyValueAndDetectChanges(value, ref _keepLatestVersionPerDayForDays, nameof(KeepLatestVersionPerDayForDays));
    }
}
