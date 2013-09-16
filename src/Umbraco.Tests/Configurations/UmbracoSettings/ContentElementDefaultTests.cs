using System.Linq;
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

        public override void DisableHtmlEmail()
        {
            Assert.IsTrue(Section.Content.DisableHtmlEmail == false);
        }

        [Test]
        public override void Can_Set_Multiple()
        {
            Assert.IsTrue(Section.Content.Error404Collection.Count() == 1);
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(0).Culture == null);
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(0).ContentId == 1);
        }

        [Test]
        public override void ImageAutoFillProperties()
        {
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.Count() == 1);
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).Alias == "umbracoFile");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias == "umbracoWidth");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias == "umbracoHeight");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias == "umbracoBytes");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias == "umbracoExtension");
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