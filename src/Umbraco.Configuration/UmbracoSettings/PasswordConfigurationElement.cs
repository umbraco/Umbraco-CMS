using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class PasswordConfigurationElement : UmbracoConfigurationElement
    {
        [ConfigurationProperty("requiredLength", DefaultValue = "12")]
        public int RequiredLength => (int)base["requiredLength"];

        [ConfigurationProperty("requireNonLetterOrDigit", DefaultValue = "false")]
        public bool RequireNonLetterOrDigit => (bool)base["requireNonLetterOrDigit"];

        [ConfigurationProperty("requireDigit", DefaultValue = "false")]
        public bool RequireDigit => (bool)base["requireDigit"];

        [ConfigurationProperty("requireLowercase", DefaultValue = "false")]
        public bool RequireLowercase => (bool)base["requireLowercase"];

        [ConfigurationProperty("requireUppercase", DefaultValue = "false")]
        public bool RequireUppercase => (bool)base["requireUppercase"];

        [ConfigurationProperty("useLegacyEncoding", DefaultValue = "false")]
        public bool UseLegacyEncoding => (bool)base["useLegacyEncoding"];

        [ConfigurationProperty("hashAlgorithmType", DefaultValue = "HMACSHA256")]
        public string HashAlgorithmType => (string)base["hashAlgorithmType"];

        [ConfigurationProperty("maxFailedAccessAttemptsBeforeLockout", DefaultValue = "5")]
        public int MaxFailedAccessAttemptsBeforeLockout => (int)base["maxFailedAccessAttemptsBeforeLockout"];        
    }
}
