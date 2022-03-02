using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.ContentEditing
{
    [DataContract(Name = "historyCleanup", Namespace = "")]
    public class HistoryCleanup : BeingDirtyBase
    {
        private bool _preventCleanup;

        [DataMember(Name = "preventCleanup")]
        public bool PreventCleanup
        {
            get => _preventCleanup;
            set => SetPropertyValueAndDetectChanges(value, ref _preventCleanup, nameof(PreventCleanup));
        }

        private int? _keepAllVersionsNewerThanDays;

        [DataMember(Name = "keepAllVersionsNewerThanDays")]
        public int? KeepAllVersionsNewerThanDays
        {
            get => _keepAllVersionsNewerThanDays;
            set => SetPropertyValueAndDetectChanges(value, ref _keepAllVersionsNewerThanDays, nameof(KeepAllVersionsNewerThanDays));
        }

        private int? _keepLatestVersionPerDayForDays;

        [DataMember(Name = "keepLatestVersionPerDayForDays")]
        public int? KeepLatestVersionPerDayForDays
        {
            get => _keepLatestVersionPerDayForDays;
            set => SetPropertyValueAndDetectChanges(value, ref _keepLatestVersionPerDayForDays,
                nameof(KeepLatestVersionPerDayForDays));
        }
    }
}
