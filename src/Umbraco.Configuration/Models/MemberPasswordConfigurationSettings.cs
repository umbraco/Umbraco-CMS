using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class MemberPasswordConfigurationSettings : IMemberPasswordConfiguration
    {
        private readonly IConfiguration _configuration;

        public MemberPasswordConfigurationSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int RequiredLength =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:RequiredLength", 10);

        public bool RequireNonLetterOrDigit =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:RequireNonLetterOrDigit", false);

        public bool RequireDigit =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:RequireDigit", false);

        public bool RequireLowercase =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:RequireLowercase", false);

        public bool RequireUppercase =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:RequireUppercase", false);

        public bool UseLegacyEncoding =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:UseLegacyEncoding", false);

        public string HashAlgorithmType =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:HashAlgorithmType", "HMACSHA256");

        public int MaxFailedAccessAttemptsBeforeLockout =>
            _configuration.GetValue("Umbraco:CMS:Security:MemberPassword:MaxFailedAccessAttemptsBeforeLockout", 5);
    }
}
