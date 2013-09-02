using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentImagingElementDefaultTests : ContentImagingElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void ImageAutoFillProperties()
        {
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.Count == 1);
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).Alias == "umbracoFile");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias == "umbracoWidth");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias == "umbracoHeight");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias == "umbracoBytes");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias == "umbracoExtension");
        }
    }
}