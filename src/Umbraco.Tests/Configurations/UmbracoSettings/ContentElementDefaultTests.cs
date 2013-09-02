using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentElementDefaultTests : ContentElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        public override void TidyCharEncoding()
        {
            Assert.IsTrue(Section.Content.TidyCharEncoding == "UTF8");
        }

        public override void XmlContentCheckForDiskChanges()
        {
            Assert.IsTrue(Section.Content.XmlContentCheckForDiskChanges == false);
        }
    }
}