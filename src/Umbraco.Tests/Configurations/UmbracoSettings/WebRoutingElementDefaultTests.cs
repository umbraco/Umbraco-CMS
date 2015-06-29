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
            Assert.IsTrue(SettingsSection.WebRouting.UrlProviderMode == "AutoLegacy");
        }

        [Test]
        public void DisableAlternativeTemplates()
        {
            Assert.IsTrue(SettingsSection.WebRouting.DisableAlternativeTemplates == false);
        }

        [Test]
        public void DisableFindContentByIdPath()
        {
            Assert.IsTrue(SettingsSection.WebRouting.DisableFindContentByIdPath == false);
        }
    }
}