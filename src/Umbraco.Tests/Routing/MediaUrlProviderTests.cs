﻿using System;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.PropertyEditors;
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

            var logger = Mock.Of<ILogger>();
            var mediaFileSystemMock = Mock.Of<IMediaFileSystem>();
            var contentSettings = new ContentSettingsBuilder().Build();
            var dataTypeService = Mock.Of<IDataTypeService>();
            var propertyEditors = new MediaUrlGeneratorCollection(new IMediaUrlGenerator[]
            {
                new FileUploadPropertyEditor(logger, mediaFileSystemMock, Microsoft.Extensions.Options.Options.Create(contentSettings), dataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper),
                new ImageCropperPropertyEditor(logger, mediaFileSystemMock, Microsoft.Extensions.Options.Options.Create(contentSettings), dataTypeService, LocalizationService, IOHelper, ShortStringHelper, LocalizedTextService),
            });
            _mediaUrlProvider = new DefaultMediaUrlProvider(propertyEditors, UriUtility);
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

            var umbracoContext = GetUmbracoContext("/");
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.UploadField, expected, null);

            var resolvedUrl = GetPublishedUrlProvider(umbracoContext).GetMediaUrl(publishedContent, UrlMode.Auto);

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

            var umbracoContext = GetUmbracoContext("/");
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.ImageCropper, imageCropperValue, configuration);

            var resolvedUrl = GetPublishedUrlProvider(umbracoContext).GetMediaUrl(publishedContent, UrlMode.Auto);

            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Can_Resolve_Absolute_Url()
        {
            const string mediaUrl = "/media/rfeiw584/test.jpg";
            var expected = $"http://localhost{mediaUrl}";

            var umbracoContext = GetUmbracoContext("http://localhost");
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.UploadField, mediaUrl, null);

            var resolvedUrl = GetPublishedUrlProvider(umbracoContext).GetMediaUrl(publishedContent, UrlMode.Absolute);

            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Returns_Absolute_Url_If_Stored_Url_Is_Absolute()
        {
            const string expected = "http://localhost/media/rfeiw584/test.jpg";

            var umbracoContext = GetUmbracoContext("http://localhost");
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.UploadField, expected, null);

            var resolvedUrl = GetPublishedUrlProvider(umbracoContext).GetMediaUrl(publishedContent, UrlMode.Relative);

            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Returns_Empty_String_When_PropertyType_Is_Not_Supported()
        {
            var umbracoContext = GetUmbracoContext("/");
            var publishedContent = CreatePublishedContent(Constants.PropertyEditors.Aliases.Boolean, "0", null);

            var resolvedUrl = GetPublishedUrlProvider(umbracoContext).GetMediaUrl(publishedContent, UrlMode.Absolute, propertyAlias: "test");

            Assert.AreEqual(string.Empty, resolvedUrl);
        }

        [Test]
        public void Get_Media_Url_Can_Resolve_Variant_Property_Url()
        {
            var umbracoContext = GetUmbracoContext("http://localhost");

            var umbracoFilePropertyType = CreatePropertyType(Constants.PropertyEditors.Aliases.UploadField, null, ContentVariation.Culture);

            const string enMediaUrl = "/media/rfeiw584/en.jpg";
            const string daMediaUrl = "/media/uf8ewud2/da.jpg";

            var property = new SolidPublishedPropertyWithLanguageVariants
            {
                Alias = "umbracoFile",
                PropertyType = umbracoFilePropertyType,
            };

            property.SetSourceValue("en", enMediaUrl, true);
            property.SetSourceValue("da", daMediaUrl);

            var contentType = new PublishedContentType(Guid.NewGuid(), 666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), new [] { umbracoFilePropertyType }, ContentVariation.Culture);
            var publishedContent = new SolidPublishedContent(contentType) {Properties = new[] {property}};

            var resolvedUrl = GetPublishedUrlProvider(umbracoContext).GetMediaUrl(publishedContent, UrlMode.Auto, "da");
            Assert.AreEqual(daMediaUrl, resolvedUrl);
        }

        private IPublishedUrlProvider GetPublishedUrlProvider(IUmbracoContext umbracoContext)
        {
            var webRoutingSettings = new WebRoutingSettingsBuilder().Build();
            return new UrlProvider(
                new TestUmbracoContextAccessor(umbracoContext),
                Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                new UrlProviderCollection(Enumerable.Empty<IUrlProvider>()),
                new MediaUrlProviderCollection(new []{_mediaUrlProvider}),
                Mock.Of<IVariationContextAccessor>()
            );
        }

        private static IPublishedContent CreatePublishedContent(string propertyEditorAlias, string propertyValue, object dataTypeConfiguration)
        {
            var umbracoFilePropertyType = CreatePropertyType(propertyEditorAlias, dataTypeConfiguration, ContentVariation.Nothing);

            var contentType = new PublishedContentType(Guid.NewGuid(), 666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(),
                new[] {umbracoFilePropertyType}, ContentVariation.Nothing);

            return new SolidPublishedContent(contentType)
            {
                Id = 1234,
                Key = Guid.NewGuid(),
                Properties = new[]
                {
                    new SolidPublishedProperty
                    {
                        Alias = "umbracoFile",
                        SolidSourceValue = propertyValue,
                        SolidHasValue = true,
                        PropertyType = umbracoFilePropertyType
                    }
                }
            };
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
