// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for member password settings.
    /// </summary>
    public class MemberPasswordConfigurationSettings : IPasswordConfiguration
    {
        /// <inheritdoc/>
        public int RequiredLength { get; set; } = 10;

        /// <inheritdoc/>
        public bool RequireNonLetterOrDigit { get; set; } = false;

        /// <inheritdoc/>
        public bool RequireDigit { get; set; } = false;

        /// <inheritdoc/>
        public bool RequireLowercase { get; set; } = false;

        /// <inheritdoc/>
        public bool RequireUppercase { get; set; } = false;

        /// <inheritdoc/>
        public string HashAlgorithmType { get; set; } = Constants.Security.AspNetCoreV3PasswordHashAlgorithmName;

        /// <inheritdoc/>
        public int MaxFailedAccessAttemptsBeforeLockout { get; set; } = 5;
    }
}
