using Umbraco.Core.Configuration;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestUserPasswordConfig : IUserPasswordConfiguration
    {
        public int RequiredLength => 12;

        public bool RequireNonLetterOrDigit => false;

        public bool RequireDigit => false;

        public bool RequireLowercase => false;

        public bool RequireUppercase => false;

        public bool UseLegacyEncoding => false;

        public string HashAlgorithmType => "HMACSHA256";

        public int MaxFailedAccessAttemptsBeforeLockout => 5;
    }
}
