using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentVersionCleanupPolicyGlobalSettingsElement : UmbracoConfigurationElement, IContentVersionCleanupPolicyGlobalSettings
    {
        [ConfigurationProperty("enable", DefaultValue = false)]
        public bool EnableCleanup => (bool)this["enable"];

        [ConfigurationProperty("keepAllVersionsNewerThanDays", DefaultValue = 2)]
        public int KeepAllVersionsNewerThanDays => (int)this["keepAllVersionsNewerThanDays"];

        [ConfigurationProperty("keepLatestVersionPerDayForDays", DefaultValue = 30)]
        public int KeepLatestVersionPerDayForDays => (int)this["keepLatestVersionPerDayForDays"];
    }
}
