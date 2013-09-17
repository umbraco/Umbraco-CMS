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

    }
}