using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    [TestFixture]
    public class WebRoutingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void TrySkipIisCustomErrors()
        {
            Assert.IsTrue(WebRoutingSettings.TrySkipIisCustomErrors == false);
        }

        [Test]
        public void InternalRedirectPreservesTemplate()
        {
            Assert.IsTrue(WebRoutingSettings.InternalRedirectPreservesTemplate == false);
        }

        [Test]
        public virtual void UrlProviderMode()
        {
            Assert.IsTrue(WebRoutingSettings.UrlProviderMode == "Auto");
        }
    }
}
