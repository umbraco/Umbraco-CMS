using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class UserPasswordConfigurationSettings : IUserPasswordConfiguration
    {
        private const string Prefix = Constants.Configuration.ConfigSecurityPrefix + "UserPassword:";
        private readonly IConfiguration _configuration;

        public UserPasswordConfigurationSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int RequiredLength => _configuration.GetValue(Prefix + "RequiredLength", 10);

        public bool RequireNonLetterOrDigit =>
            _configuration.GetValue(Prefix + "RequireNonLetterOrDigit", false);

        public bool RequireDigit => _configuration.GetValue(Prefix + "RequireDigit", false);

        public bool RequireLowercase =>
            _configuration.GetValue(Prefix + "RequireLowercase", false);

        public bool RequireUppercase =>
            _configuration.GetValue(Prefix + "RequireUppercase", false);

        public string HashAlgorithmType =>
            _configuration.GetValue(Prefix + "HashAlgorithmType", "HMACSHA256");

        public int MaxFailedAccessAttemptsBeforeLockout =>
            _configuration.GetValue(Prefix + "MaxFailedAccessAttemptsBeforeLockout", 5);
    }
}
