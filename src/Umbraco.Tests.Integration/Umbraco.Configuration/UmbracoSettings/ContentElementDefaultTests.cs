using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    [TestFixture]
    public class ContentElementDefaultTests : ContentElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void DisableHtmlEmail()
        {
            Assert.IsTrue(ContentSettings.DisableHtmlEmail == false);
        }

        [Test]
        public override void Can_Set_Multiple()
        {
            Assert.IsTrue(ContentSettings.Error404Collection.Count() == 1);
            Assert.IsTrue(ContentSettings.Error404Collection.ElementAt(0).Culture == null);
            Assert.IsTrue(ContentSettings.Error404Collection.ElementAt(0).ContentId == 1);
        }

        [Test]
        public override void ImageAutoFillProperties()
        {
            Assert.IsTrue(ContentSettings.ImageAutoFillProperties.Count() == 1);
            Assert.IsTrue(ContentSettings.ImageAutoFillProperties.ElementAt(0).Alias == "umbracoFile");
            Assert.IsTrue(ContentSettings.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias == "umbracoWidth");
            Assert.IsTrue(ContentSettings.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias == "umbracoHeight");
            Assert.IsTrue(ContentSettings.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias == "umbracoBytes");
            Assert.IsTrue(ContentSettings.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias == "umbracoExtension");
        }

    }
}
