using Umbraco.Core;
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
    public abstract class PublishedContentTestBase : BaseRoutingTest
    {
        public override void Initialize()
        {
            base.Initialize();
            
            // need to specify a custom callback for unit tests
            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("content", 0, Constants.PropertyEditors.TinyMCEAlias), 
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = (alias) => type;

            var rCtx = GetRoutingContext("/test", 1234);
            Umbraco.Web.Current.SetUmbracoContext(rCtx.UmbracoContext, true);            
        }

        protected override void FreezeResolution()
        {
            if (PropertyValueConvertersResolver.HasCurrent == false)
                PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(
                    Container, Logger,
                    new[]
                        {
                            typeof(DatePickerValueConverter),
                            typeof(TinyMceValueConverter),
                            typeof(YesNoValueConverter)
                        });    

            if (PublishedContentModelFactoryResolver.HasCurrent == false)
                PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();

            base.FreezeResolution();
        }
    }
}