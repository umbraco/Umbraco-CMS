using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class WebRoutingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void TrySkipIisCustomErrors()
        {
            Assert.IsTrue(Section.WebRouting.TrySkipIisCustomErrors == false);
        }

        [Test]
        public void InternalRedirectPreservesTemplate()
        {
            Assert.IsTrue(Section.WebRouting.TrySkipIisCustomErrors == false);
        }
    }

    [TestFixture]
    public class WebRoutingElementDefaultTests : WebRoutingElementTests
    {

        protected override bool TestingDefaults
        {
            get { return true; }
        }

    }
}