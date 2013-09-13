using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class SecurityElementTests : UmbracoSettingsTests
    {
        [Test]
        public void KeepUserLoggedIn()
        {
            Assert.IsTrue(Section.Security.KeepUserLoggedIn == true);
        }
        [Test]
        public void HideDisabledUsersInBackoffice()
        {
            Assert.IsTrue(Section.Security.HideDisabledUsersInBackoffice == false);
        }
        [Test]
        public void AuthCookieDomain()
        {
            Assert.IsTrue(Section.Security.AuthCookieDomain == null);
        }
        [Test]
        public void AuthCookieName()
        {
            Assert.IsTrue(Section.Security.AuthCookieName == "UMB_UCONTEXT");
        }
    }
}