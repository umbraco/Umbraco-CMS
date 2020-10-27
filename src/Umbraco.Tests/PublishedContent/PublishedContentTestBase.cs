using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.TestHelpers;
using Moq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Templates;
using Umbraco.Web.Routing;
using Umbraco.Core.Media;
using System;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
    /// Abstract base class for tests for published content and published media
    /// </summary>
    public abstract class PublishedContentTestBase : BaseWebTest
    {
        protected override void Compose()
        {
            base.Compose();

            // FIXME: what about the if (PropertyValueConvertersResolver.HasCurrent == false) ??
            // can we risk double - registering and then, what happens?

            Composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Clear()
                .Append<DatePickerValueConverter>()
                .Append<TinyMceValueConverter>()
                .Append<YesNoValueConverter>();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var converters = Factory.GetRequiredService<PropertyValueConverterCollection>();
            var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
            var publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
            var loggerFactory = NullLoggerFactory.Instance;

            var imageSourceParser = new HtmlImageSourceParser(publishedUrlProvider);
            var pastedImages = new RichTextEditorPastedImages(umbracoContextAccessor, loggerFactory.CreateLogger<RichTextEditorPastedImages>(), IOHelper,  Mock.Of<IMediaService>(), Mock.Of<IContentTypeBaseServiceProvider>(), Mock.Of<IMediaFileSystem>(), ShortStringHelper, publishedUrlProvider);
            var localLinkParser = new HtmlLocalLinkParser(umbracoContextAccessor, publishedUrlProvider);
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new RichTextPropertyEditor(
                    loggerFactory,
                    Mock.Of<IUmbracoContextAccessor>(),
                    Mock.Of<IDataTypeService>(),
                    Mock.Of<ILocalizationService>(),
                    imageSourceParser,
                    localLinkParser,
                    pastedImages,
                    ShortStringHelper,
                    IOHelper,
                    LocalizedTextService,
                    Mock.Of<IImageUrlGenerator>())) { Id = 1 });


            var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeService);

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
            {
                yield return publishedContentTypeFactory.CreatePropertyType(contentType, "content", 1);
            }

            var type = new AutoPublishedContentType(Guid.NewGuid(), 0, "anything", CreatePropertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => type;

            var umbracoContext = GetUmbracoContext("/test");
            Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
        }
    }
}
