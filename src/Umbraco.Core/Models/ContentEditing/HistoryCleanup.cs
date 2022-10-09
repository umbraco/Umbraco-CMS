using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "historyCleanup", Namespace = "")]
public class HistoryCleanup : BeingDirtyBase
{
    private int? _keepAllVersionsNewerThanDays;
    private int? _keepLatestVersionPerDayForDays;
    private bool _preventCleanup;

    [DataMember(Name = "preventCleanup")]
    public bool PreventCleanup
    {
        get => _preventCleanup;
        set => SetPropertyValueAndDetectChanges(value, ref _preventCleanup, nameof(PreventCleanup));
    }

    [DataMember(Name = "keepAllVersionsNewerThanDays")]
    public int? KeepAllVersionsNewerThanDays
    {
        get => _keepAllVersionsNewerThanDays;
        set => SetPropertyValueAndDetectChanges(value, ref _keepAllVersionsNewerThanDays, nameof(KeepAllVersionsNewerThanDays));
    }

    [DataMember(Name = "keepLatestVersionPerDayForDays")]
    public int? KeepLatestVersionPerDayForDays
    {
        get => _keepLatestVersionPerDayForDays;
        set => SetPropertyValueAndDetectChanges(value, ref _keepLatestVersionPerDayForDays, nameof(KeepLatestVersionPerDayForDays));
    }
}
