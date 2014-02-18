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

        [Test]
        public override void DisableHtmlEmail()
        {
            Assert.IsTrue(SettingsSection.Content.DisableHtmlEmail == false);
        }

        [Test]
        public override void Can_Set_Multiple()
        {
            Assert.IsTrue(SettingsSection.Content.Error404Collection.Count() == 1);
            Assert.IsTrue(SettingsSection.Content.Error404Collection.ElementAt(0).Culture == null);
            Assert.IsTrue(SettingsSection.Content.Error404Collection.ElementAt(0).ContentId == 1);
        }

        [Test]
        public override void ImageAutoFillProperties()
        {
            Assert.IsTrue(SettingsSection.Content.ImageAutoFillProperties.Count() == 1);
            Assert.IsTrue(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).Alias == "umbracoFile");
            Assert.IsTrue(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias == "umbracoWidth");
            Assert.IsTrue(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias == "umbracoHeight");
            Assert.IsTrue(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias == "umbracoBytes");
            Assert.IsTrue(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias == "umbracoExtension");
        }

        [Test]
        public override void TidyCharEncoding()
        {
            Assert.IsTrue(SettingsSection.Content.TidyCharEncoding == "UTF8");
        }

        [Test]
        public override void XmlContentCheckForDiskChanges()
        {
            Assert.IsTrue(SettingsSection.Content.XmlContentCheckForDiskChanges == false);
        }
    }
}