namespace Umbraco.Core.Configuration.Models
{
    public class UserPasswordConfigurationSettings : IPasswordConfiguration
    {
        public int RequiredLength { get; set; } = 10;

        public bool RequireNonLetterOrDigit { get; set; } = false;

        public bool RequireDigit { get; set; } = false;

        public bool RequireLowercase { get; set; } = false;

        public bool RequireUppercase { get; set; } = false;

        public string HashAlgorithmType { get; set; } = Constants.Security.AspNetCoreV3PasswordHashAlgorithmName;

        public int MaxFailedAccessAttemptsBeforeLockout { get; set; } = 5;
    }
}
