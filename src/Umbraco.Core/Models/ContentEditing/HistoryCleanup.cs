using System.Runtime.Serialization;

namespace Umbraco.Core.Models.ContentEditing
{
    [DataContract(Name = "historyCleanup", Namespace = "")]
    public class HistoryCleanup
    {
        [DataMember(Name = "preventCleanup")]
        public bool PreventCleanup { get; set; }

        [DataMember(Name = "keepAllVersionsNewerThanDays")]
        public int? KeepAllVersionsNewerThanDays { get; set; }

        [DataMember(Name = "keepLatestVersionPerDayForDays")]
        public int? KeepLatestVersionPerDayForDays { get; set; }
    }
}
