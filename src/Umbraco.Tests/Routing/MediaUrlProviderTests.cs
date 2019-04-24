using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class MediaUrlProviderTests : BaseWebTest
    {
        private DefaultMediaUrlProvider _mediaUrlProvider;

        public override void SetUp()
        {
            base.SetUp();

            _mediaUrlProvider = new DefaultMediaUrlProvider();
        }

        public override void TearDown()
        {
            base.TearDown();

            _mediaUrlProvider = null;
        }

        [Test]
        public void Get_Media_Url_Resolves_Url_From_Upload_Property_Editor()
        {
            const string expected = "/media/rfeiw584/test.jpg";

            var umbracoContext = GetUmbracoContext("/", mediaUrlProviders: new[] { _mediaUrlProvider });
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.UploadField, expected, null);

            var resolvedUrl = umbracoContext.UrlProvider.GetMediaUrl(publishedContent, "umbracoFile", UrlMode.Auto, null, null);

            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Resolves_Url_From_Image_Cropper_Property_Editor()
        {
            const string expected = "/media/rfeiw584/test.jpg";

            var configuration = new ImageCropperConfiguration();
            var imageCropperValue = JsonConvert.SerializeObject(new ImageCropperValue
            {
                Src = expected
            });

            var umbracoContext = GetUmbracoContext("/", mediaUrlProviders: new[] { _mediaUrlProvider });
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.ImageCropper, imageCropperValue, configuration);

            var resolvedUrl = umbracoContext.UrlProvider.GetMediaUrl(publishedContent, "umbracoFile", UrlMode.Auto, null, null);

            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Can_Resolve_Absolute_Url()
        {
            const string mediaUrl = "/media/rfeiw584/test.jpg";
            var expected = $"http://localhost{mediaUrl}";

            var umbracoContext = GetUmbracoContext("http://localhost", mediaUrlProviders: new[] { _mediaUrlProvider });
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.UploadField, mediaUrl, null);

            var resolvedUrl = umbracoContext.UrlProvider.GetMediaUrl(publishedContent, "umbracoFile", UrlMode.Absolute, null, null);

            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Returns_Empty_String_When_PropertyType_Is_Not_Supported()
        {
            var umbracoContext = GetUmbracoContext("/", mediaUrlProviders: new[] { _mediaUrlProvider });
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.Boolean, "0", null);

            var resolvedUrl = umbracoContext.UrlProvider.GetMediaUrl(publishedContent, "test", UrlMode.Absolute, null, null);

            Assert.AreEqual(string.Empty, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Can_Resolve_Variant_Property_Url()
        {
            var umbracoContext = GetUmbracoContext("http://localhost", mediaUrlProviders: new[] { _mediaUrlProvider });

            var umbracoFilePropertyType = CreatePropertyType(Constants.PropertyEditors.Aliases.UploadField, null, ContentVariation.Culture);

            const string enMediaUrl = "/media/rfeiw584/en.jpg";
            const string daMediaUrl = "/media/uf8ewud2/da.jpg";

            var property = new SolidPublishedPropertyWithLanguageVariants
            {
                Alias = "umbracoFile",
                PropertyType = umbracoFilePropertyType,
            };

            property.SetValue("en", enMediaUrl, true);
            property.SetValue("da", daMediaUrl);

            var contentType = new PublishedContentType(666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), new [] { umbracoFilePropertyType }, ContentVariation.Culture);
            var publishedContent = new SolidPublishedContent(contentType) {Properties = new[] {property}};

            var resolvedUrl = umbracoContext.UrlProvider.GetMediaUrl(publishedContent, "umbracoFile", UrlMode.Auto, "da", null);
            Assert.AreEqual(daMediaUrl, resolvedUrl);
        }

        private static TestPublishedContent CreatePublishedContent(string propertyEditorAlias, object propertyValue, object dataTypeConfiguration)
        {
            var umbracoFilePropertyType = CreatePropertyType(propertyEditorAlias, dataTypeConfiguration, ContentVariation.Nothing);

            var contentType = new PublishedContentType(666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(),
                new[] {umbracoFilePropertyType}, ContentVariation.Nothing);

            return new TestPublishedContent(contentType, 1234, Guid.NewGuid(),
                new Dictionary<string, object> {{"umbracoFile", propertyValue } }, false);
        }

        private static PublishedPropertyType CreatePropertyType(string propertyEditorAlias, object dataTypeConfiguration, ContentVariation variation)
        {
            var uploadDataType = new PublishedDataType(1234, propertyEditorAlias, new Lazy<object>(() => dataTypeConfiguration));

            var propertyValueConverters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                new UploadPropertyConverter(),
                new ImageCropperValueConverter(),
            });

            var publishedModelFactory = Mock.Of<IPublishedModelFactory>();
            var publishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
            publishedContentTypeFactory.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(uploadDataType);

            return new PublishedPropertyType("umbracoFile", 42, true, variation, propertyValueConverters, publishedModelFactory, publishedContentTypeFactory.Object);
        }
    }
}
