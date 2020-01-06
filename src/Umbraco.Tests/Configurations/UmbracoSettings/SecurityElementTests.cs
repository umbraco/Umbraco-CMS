using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class SecurityElementTests : UmbracoSettingsTests
    {
        [Test]
        public void KeepUserLoggedIn()
        {
            Assert.IsTrue(SettingsSection.Security.KeepUserLoggedIn == true);
        }

        [Test]
        public void HideDisabledUsersInBackoffice()
        {
            Assert.IsTrue(SettingsSection.Security.HideDisabledUsersInBackoffice == false);
        }

        [Test]
        public void AllowPasswordReset()
        {
            Assert.IsTrue(SettingsSection.Security.AllowPasswordReset == true);
        }

        [Test]
        public void AuthCookieDomain()
        {
            Assert.IsTrue(SettingsSection.Security.AuthCookieDomain == null);
        }

        [Test]
        public void AuthCookieName()
        {
            Assert.IsTrue(SettingsSection.Security.AuthCookieName == "UMB_UCONTEXT");
        }

        [Test]
        public void UserPasswordConfiguration_RequiredLength()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.RequiredLength == 12);
        }

        [Test]
        public void UserPasswordConfiguration_RequireNonLetterOrDigit()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.RequireNonLetterOrDigit == false);
        }

        [Test]
        public void UserPasswordConfiguration_RequireDigit()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.RequireDigit == false);
        }

        [Test]
        public void UserPasswordConfiguration_RequireLowercase()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.RequireLowercase == false);
        }

        [Test]
        public void UserPasswordConfiguration_RequireUppercase()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.RequireUppercase == false);
        }

        [Test]
        public void UserPasswordConfiguration_UseLegacyEncoding()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.UseLegacyEncoding == false);
        }

        [Test]
        public void UserPasswordConfiguration_HashAlgorithmType()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.HashAlgorithmType == "HMACSHA256");
        }

        [Test]
        public void UserPasswordConfiguration_MaxFailedAccessAttemptsBeforeLockout()
        {
            Assert.IsTrue(SettingsSection.Security.UserPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout == 5);
        }

        [Test]
        public void MemberPasswordConfiguration_RequiredLength()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.RequiredLength == 12);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireNonLetterOrDigit()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.RequireNonLetterOrDigit == false);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireDigit()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.RequireDigit == false);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireLowercase()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.RequireLowercase == false);
        }

        [Test]
        public void MemberPasswordConfiguration_RequireUppercase()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.RequireUppercase == false);
        }

        [Test]
        public void MemberPasswordConfiguration_UseLegacyEncoding()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.UseLegacyEncoding == false);
        }

        [Test]
        public void MemberPasswordConfiguration_HashAlgorithmType()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.HashAlgorithmType == "HMACSHA256");
        }

        [Test]
        public void MemberPasswordConfiguration_MaxFailedAccessAttemptsBeforeLockout()
        {
            Assert.IsTrue(SettingsSection.Security.MemberPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout == 5);
        }
    }
}
