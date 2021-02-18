using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Routing;

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

            Builder.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Clear()
                .Append<DatePickerValueConverter>()
                .Append<SimpleTinyMceValueConverter>()
                .Append<YesNoValueConverter>();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var converters = Factory.GetRequiredService<PropertyValueConverterCollection>();
            var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
            var publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
            var loggerFactory = NullLoggerFactory.Instance;
            var serializer = new ConfigurationEditorJsonSerializer();

            var imageSourceParser = new HtmlImageSourceParser(publishedUrlProvider);
            var pastedImages = new RichTextEditorPastedImages(umbracoContextAccessor, loggerFactory.CreateLogger<RichTextEditorPastedImages>(), HostingEnvironment,  Mock.Of<IMediaService>(), Mock.Of<IContentTypeBaseServiceProvider>(), Mock.Of<IMediaFileSystem>(), ShortStringHelper, publishedUrlProvider, serializer);
            var localLinkParser = new HtmlLocalLinkParser(umbracoContextAccessor, publishedUrlProvider);
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new RichTextPropertyEditor(
                    loggerFactory,
                    Mock.Of<IBackOfficeSecurityAccessor>(),
                    Mock.Of<IDataTypeService>(),
                    Mock.Of<ILocalizationService>(),
                    imageSourceParser,
                    localLinkParser,
                    pastedImages,
                    ShortStringHelper,
                    IOHelper,
                    LocalizedTextService,
                    Mock.Of<IImageUrlGenerator>(),
                    new JsonNetSerializer()),
                    serializer) { Id = 1 });


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
