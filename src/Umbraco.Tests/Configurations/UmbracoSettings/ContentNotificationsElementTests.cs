using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentNotificationsElementTests : UmbracoSettingsTests
    {
        [Test]
        public void EmailAddress()
        {
            Assert.IsTrue(Section.Content.Notifications.EmailAddress == "robot@umbraco.dk");
        }
        [Test]
        public virtual void DisableHtmlEmail()
        {
            Assert.IsTrue(Section.Content.Notifications.DisableHtmlEmail == true);
        }
    }
}