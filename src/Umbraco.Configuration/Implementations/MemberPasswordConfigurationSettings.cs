using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Implementations
{
    internal class MemberPasswordConfigurationSettings : ConfigurationManagerConfigBase, IMemberPasswordConfiguration
    {
        public int RequiredLength => UmbracoSettingsSection.Security.UserPasswordConfiguration.RequiredLength;
        public bool RequireNonLetterOrDigit => UmbracoSettingsSection.Security.UserPasswordConfiguration.RequireNonLetterOrDigit;
        public bool RequireDigit => UmbracoSettingsSection.Security.UserPasswordConfiguration.RequireDigit;
        public bool RequireLowercase=> UmbracoSettingsSection.Security.UserPasswordConfiguration.RequireLowercase;
        public bool RequireUppercase=> UmbracoSettingsSection.Security.UserPasswordConfiguration.RequireUppercase;
        public bool UseLegacyEncoding=> UmbracoSettingsSection.Security.UserPasswordConfiguration.UseLegacyEncoding;
        public string HashAlgorithmType=> UmbracoSettingsSection.Security.UserPasswordConfiguration.HashAlgorithmType;
        public int MaxFailedAccessAttemptsBeforeLockout => UmbracoSettingsSection.Security.UserPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
    }
}
