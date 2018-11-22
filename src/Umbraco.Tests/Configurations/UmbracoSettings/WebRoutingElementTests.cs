using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class WebRoutingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void TrySkipIisCustomErrors()
        {
            Assert.IsTrue(SettingsSection.WebRouting.TrySkipIisCustomErrors == false);
        }

        [Test]
        public void InternalRedirectPreservesTemplate()
        {
            Assert.IsTrue(SettingsSection.WebRouting.TrySkipIisCustomErrors == false);
        }

        [Test]
        public virtual void UrlProviderMode()
        {
            Assert.IsTrue(SettingsSection.WebRouting.UrlProviderMode == "AutoLegacy");
        }
    }
}