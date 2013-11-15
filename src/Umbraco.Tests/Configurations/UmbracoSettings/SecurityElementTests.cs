using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class SecurityElementTests : UmbracoSettingsTests
    {
        [Test]
        public void KeepUserLoggedIn()
        {
            Assert.IsTrue(SettingsSection.Security.KeepUserLoggedIn == false);
        }
        [Test]
        public void HideDisabledUsersInBackoffice()
        {
            Assert.IsTrue(SettingsSection.Security.HideDisabledUsersInBackoffice == false);
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
    }
}