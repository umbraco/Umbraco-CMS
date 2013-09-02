using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentNotificationsElementDefaultTests : ContentNotificationsElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        public override void DisableHtmlEmail()
        {
            Assert.IsTrue(Section.Content.Notifications.DisableHtmlEmail == false);
        }
    }

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