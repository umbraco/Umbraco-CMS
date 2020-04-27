using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    [TestFixture]
    public class SecurityElementTests : UmbracoSettingsTests
    {
        [Test]
        public void KeepUserLoggedIn()
        {
            Assert.IsTrue(SecuritySettings.KeepUserLoggedIn == true);
        }

        [Test]
        public void HideDisabledUsersInBackoffice()
        {
            Assert.IsTrue(SecuritySettings.HideDisabledUsersInBackoffice == false);
        }

        [Test]
        public void AllowPasswordReset()
        {
            Assert.IsTrue(SecuritySettings.AllowPasswordReset == true);
        }

        [Test]
        public void AuthCookieDomain()
        {
            Assert.IsTrue(SecuritySettings.AuthCookieDomain == null);
        }

        [Test]
        public void AuthCookieName()
        {
            Assert.IsTrue(SecuritySettings.AuthCookieName == "UMB_UCONTEXT");
        }

        [Test]
        public void UserPasswordConfiguration_RequiredLength()
        {
            Assert.IsTrue(UserPasswordConfiguration.RequiredLength == 12);
        }

        [Test]
        public void UserPasswordConfiguration_RequireNonLetterOrDigit()
        {
            Assert.IsTrue(UserPasswordConfiguration.RequireNonLetterOrDigit == false);
        }

        [Test]
        public void UserPasswordConfiguration_RequireDigit()
        {
            Assert.IsTrue(UserPasswordConfiguration.RequireDigit == false);
        }

        [Test]
        public void UserPasswordConfiguration_RequireLowercase()
        {
            Assert.IsTrue(UserPasswordConfiguration.RequireLowercase == false);
        }

        [Test]
        public void UserPasswordConfiguration_RequireUppercase()
        {
            Assert.IsTrue(UserPasswordConfiguration.RequireUppercase == false);
        }

        [Test]
        public void UserPasswordConfiguration_HashAlgorithmType()
        {
            Assert.IsTrue(UserPasswordConfiguration.HashAlgorithmType == "HMACSHA256");
        }

        [Test]
        public void UserPasswordConfiguration_MaxFailedAccessAttemptsBeforeLockout()
        {
            Assert.IsTrue(UserPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout == 5);
        }

        [Test]
        public void MemberPasswordConfiguration_RequiredLength()
        {
            Assert.IsTrue(MemberPasswordConfiguration.RequiredLength == 12);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireNonLetterOrDigit()
        {
            Assert.IsTrue(MemberPasswordConfiguration.RequireNonLetterOrDigit == false);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireDigit()
        {
            Assert.IsTrue(MemberPasswordConfiguration.RequireDigit == false);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireLowercase()
        {
            Assert.IsTrue(MemberPasswordConfiguration.RequireLowercase == false);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireUppercase()
        {
            Assert.IsTrue(MemberPasswordConfiguration.RequireUppercase == false);
        }

        [Test]
        public void MemberPasswordConfiguration_HashAlgorithmType()
        {
            Assert.IsTrue(MemberPasswordConfiguration.HashAlgorithmType == "HMACSHA256");
        }

        [Test]
        public void MemberPasswordConfiguration_MaxFailedAccessAttemptsBeforeLockout()
        {
            Assert.IsTrue(MemberPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout == 5);
        }
    }
}
