using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentImagingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void ImageFileTypes()
        {
            Assert.IsTrue(Section.Content.Imaging.ImageFileTypes.All(x => "jpeg,jpg,gif,bmp,png,tiff,tif".Split(',').Contains(x)));
        }
        [Test]
        public void AllowedAttributes()
        {
            Assert.IsTrue(Section.Content.Imaging.AllowedAttributes.All(x => "src,alt,border,class,style,align,id,name,onclick,usemap".Split(',').Contains(x)));
        }
        [Test]
        public virtual void ImageAutoFillProperties()
        {
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.Count == 2);
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).Alias == "umbracoFile");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias == "umbracoWidth");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias == "umbracoHeight");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias == "umbracoBytes");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias == "umbracoExtension");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(1).Alias == "umbracoFile2");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(1).WidthFieldAlias == "umbracoWidth2");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(1).HeightFieldAlias == "umbracoHeight2");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(1).LengthFieldAlias == "umbracoBytes2");
            Assert.IsTrue(Section.Content.Imaging.ImageAutoFillProperties.ElementAt(1).ExtensionFieldAlias == "umbracoExtension2");
        }
    }
}