using System.Runtime.Serialization;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "historyCleanup", Namespace = "")]
    public class HistoryCleanupViewModel : HistoryCleanup
    {
        [DataMember(Name = "globalKeepAllVersionsNewerThanDays")]
        public int? GlobalKeepAllVersionsNewerThanDays { get;set; }

        [DataMember(Name = "globalKeepLatestVersionPerDayForDays")]
        public int? GlobalKeepLatestVersionPerDayForDays { get; set;}

        [DataMember(Name = "globalEnableCleanup")]
        public bool GlobalEnableCleanup { get; set; }
    }

}

