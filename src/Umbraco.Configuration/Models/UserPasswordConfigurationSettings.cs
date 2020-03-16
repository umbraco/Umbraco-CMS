using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class UserPasswordConfigurationSettings : IUserPasswordConfiguration
    {
        private readonly IConfiguration _configuration;
        public UserPasswordConfigurationSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int RequiredLength => _configuration.GetValue<int?>("Umbraco:CMS:Security:UserPassword:RequiredLength") ?? 10;
        public bool RequireNonLetterOrDigit =>  _configuration.GetValue<bool?>("Umbraco:CMS:Security:UserPassword:RequireNonLetterOrDigit") ?? false;
        public bool RequireDigit => _configuration.GetValue<bool?>("Umbraco:CMS:Security:UserPassword:RequireDigit") ?? false;
        public bool RequireLowercase => _configuration.GetValue<bool?>("Umbraco:CMS:Security:UserPassword:RequireLowercase") ?? false;
        public bool RequireUppercase => _configuration.GetValue<bool?>("Umbraco:CMS:Security:UserPassword:RequireUppercase") ?? false;
        public bool UseLegacyEncoding => _configuration.GetValue<bool?>("Umbraco:CMS:Security:UserPassword:UseLegacyEncoding") ?? false;
        public string HashAlgorithmType => _configuration.GetValue<string>("Umbraco:CMS:Security:UserPassword:HashAlgorithmType") ?? "HMACSHA256";
        public int MaxFailedAccessAttemptsBeforeLockout => _configuration.GetValue<int?>("Umbraco:CMS:Security:UserPassword:MaxFailedAccessAttemptsBeforeLockout") ?? 5;
    }
}
