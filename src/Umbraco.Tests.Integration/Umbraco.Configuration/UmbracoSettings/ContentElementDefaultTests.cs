using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    [TestFixture]
    public class ContentElementDefaultTests : ContentElementTests
    {
        protected override bool TestingDefaults => true;

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

        /// <summary>
        /// Whitelist is empty in default settings file and is not populated by default, but disallowed is empty and is populated by default
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="expected"></param>
        [Test]
        [TestCase("png", true)]
        [TestCase("jpg", true)]
        [TestCase("gif", true)]
        [TestCase("bmp", true)]
        [TestCase("php", true)]
        [TestCase("ashx", false)]
        [TestCase("config", false)]
        public override void IsFileAllowedForUpload_WithWhitelist(string extension, bool expected)
        {
            TestContext.WriteLine("Extension being tested: {0}", extension);
            TestContext.WriteLine("Expected IsAllowed?: {0}", expected);
            TestContext.WriteLine("AllowedUploadFiles: {0}", ContentSettings.AllowedUploadFiles);
            TestContext.WriteLine("DisallowedUploadFiles: {0}", ContentSettings.DisallowedUploadFiles);

            bool allowedContainsExtension = ContentSettings.AllowedUploadFiles.Any(x => x.InvariantEquals(extension));
            bool disallowedContainsExtension = ContentSettings.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension));

            TestContext.WriteLine("AllowedContainsExtension: {0}", allowedContainsExtension);
            TestContext.WriteLine("DisallowedContainsExtension: {0}", disallowedContainsExtension);

            Assert.AreEqual(expected, ContentSettings.IsFileAllowedForUpload(extension));
        }

    }
}
