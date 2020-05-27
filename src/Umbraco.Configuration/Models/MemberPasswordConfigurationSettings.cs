using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class MemberPasswordConfigurationSettings : IMemberPasswordConfiguration
    {
        private const string Prefix = Constants.Configuration.ConfigSecurityPrefix + "MemberPassword:";
        private readonly IConfiguration _configuration;

        public MemberPasswordConfigurationSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int RequiredLength =>
            _configuration.GetValue(Prefix + "RequiredLength", 10);

        public bool RequireNonLetterOrDigit =>
            _configuration.GetValue(Prefix + "RequireNonLetterOrDigit", false);

        public bool RequireDigit =>
            _configuration.GetValue(Prefix + "RequireDigit", false);

        public bool RequireLowercase =>
            _configuration.GetValue(Prefix + "RequireLowercase", false);

        public bool RequireUppercase =>
            _configuration.GetValue(Prefix + "RequireUppercase", false);

        public string HashAlgorithmType =>
            _configuration.GetValue(Prefix + "HashAlgorithmType", Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName); // TODO: Need to change to current format when we do members

        public int MaxFailedAccessAttemptsBeforeLockout =>
            _configuration.GetValue(Prefix + "MaxFailedAccessAttemptsBeforeLockout", 5);
    }
}
