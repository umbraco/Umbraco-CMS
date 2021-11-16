using System;

namespace Umbraco.Core.Models
{
    public class ContentVersionCleanupPolicySettings
    {
        public int ContentTypeId { get; set; }
        public int? KeepAllVersionsNewerThanDays { get; set; }
        public int? KeepLatestVersionPerDayForDays { get; set; }
        public bool PreventCleanup { get; set; }
        public DateTime Updated { get; set; }
    }
}
