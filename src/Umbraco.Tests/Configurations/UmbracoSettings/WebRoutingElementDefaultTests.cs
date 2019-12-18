using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class WebRoutingElementDefaultTests : WebRoutingElementTests
    {

        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void UrlProviderMode()
        {
            Assert.IsTrue(SettingsSection.WebRouting.UrlProviderMode == "Auto");
        }

        [Test]
        public void DisableAlternativeTemplates()
        {
            Assert.IsTrue(SettingsSection.WebRouting.DisableAlternativeTemplates == false);
        }

        [Test]
        public void ValidateAlternativeTemplates()
        {
            Assert.IsTrue(SettingsSection.WebRouting.ValidateAlternativeTemplates == false);
        }

        [Test]
        public void DisableFindContentByIdPath()
        {
            Assert.IsTrue(SettingsSection.WebRouting.DisableFindContentByIdPath == false);
        }

        [Test]
        public void DisableRedirectUrlTracking()
        {
            Assert.IsTrue(SettingsSection.WebRouting.DisableRedirectUrlTracking == false);
        }
    }
}
