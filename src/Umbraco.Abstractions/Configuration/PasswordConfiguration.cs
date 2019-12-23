using System;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    public abstract class PasswordConfiguration : IPasswordConfiguration
    {
        protected PasswordConfiguration(IPasswordConfigurationSection configSection)
        {
            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            RequiredLength = configSection.RequiredLength;
            RequireNonLetterOrDigit = configSection.RequireNonLetterOrDigit;
            RequireDigit = configSection.RequireDigit;
            RequireLowercase = configSection.RequireLowercase;
            RequireUppercase = configSection.RequireUppercase;
            UseLegacyEncoding = configSection.UseLegacyEncoding;
            HashAlgorithmType = configSection.HashAlgorithmType;
            MaxFailedAccessAttemptsBeforeLockout = configSection.MaxFailedAccessAttemptsBeforeLockout;
        }

        public int RequiredLength { get; }

        public bool RequireNonLetterOrDigit { get; }

        public bool RequireDigit { get; }

        public bool RequireLowercase { get; }

        public bool RequireUppercase { get; }

        public bool UseLegacyEncoding { get; }

        public string HashAlgorithmType { get; }

        public int MaxFailedAccessAttemptsBeforeLockout { get; }
    }
}
