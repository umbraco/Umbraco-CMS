using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.TestHelpers;
using Moq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Templates;
using Umbraco.Web.Models;
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

            var converters = Factory.GetInstance<PropertyValueConverterCollection>();
            var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
            var logger = Mock.Of<ILogger>();

            var imageSourceParser = new HtmlImageSourceParser(umbracoContextAccessor);
            var pastedImages = new RichTextEditorPastedImages(umbracoContextAccessor, logger, Mock.Of<IMediaService>(), Mock.Of<IContentTypeBaseServiceProvider>());
            var localLinkParser = new HtmlLocalLinkParser(umbracoContextAccessor);
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new RichTextPropertyEditor(logger, umbracoContextAccessor, imageSourceParser, localLinkParser, pastedImages, Mock.Of<IImageUrlGenerator>())) { Id = 1 });

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
