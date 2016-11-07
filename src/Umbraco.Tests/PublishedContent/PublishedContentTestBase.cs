using Umbraco.Core;
using Umbraco.Core.DI;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
    /// Abstract base class for tests for published content and published media
    /// </summary>
    public abstract class PublishedContentTestBase : BaseWebTest
    {
        public override void SetUp()
        {
            base.SetUp();

            // need to specify a custom callback for unit tests
            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("content", 0, Constants.PropertyEditors.TinyMCEAlias),
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = (alias) => type;

            var umbracoContext = GetUmbracoContext("/test");
            Umbraco.Web.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
        }

        protected override void Compose()
        {
            base.Compose();

            // fixme - what about the if (PropertyValueConvertersResolver.HasCurrent == false) ??
            // can we risk double - registering and then, what happens?
            Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append<DatePickerValueConverter>()
                .Append<TinyMceValueConverter>()
                .Append<YesNoValueConverter>();
        }
    }
}