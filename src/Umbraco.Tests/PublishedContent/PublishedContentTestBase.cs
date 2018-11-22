using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.TestHelpers;
using LightInject;
using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;

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

            // fixme - what about the if (PropertyValueConvertersResolver.HasCurrent == false) ??
            // can we risk double - registering and then, what happens?

            var builder = Container.TryGetInstance<PropertyValueConverterCollectionBuilder>()
                ?? Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>();

            builder.Clear()
                .Append<DatePickerValueConverter>()
                .Append<TinyMceValueConverter>()
                .Append<YesNoValueConverter>();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var converters = Container.GetInstance<PropertyValueConverterCollection>();

            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new RichTextPropertyEditor(Mock.Of<ILogger>())) { Id = 1 });

            var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeService);

            // need to specify a custom callback for unit tests
            var propertyTypes = new[]
            {
                // AutoPublishedContentType will auto-generate other properties
                publishedContentTypeFactory.CreatePropertyType("content", 1),
            };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => type;

            var umbracoContext = GetUmbracoContext("/test");
            Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
        }
    }
}
