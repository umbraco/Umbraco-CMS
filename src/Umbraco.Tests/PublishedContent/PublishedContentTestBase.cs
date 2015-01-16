using System;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

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
            PublishedContentType.GetPublishedContentTypeCallback = (alias) => type;

            var rCtx = GetRoutingContext("/test", 1234);
            UmbracoContext.Current = rCtx.UmbracoContext;
            
        }

        protected override void FreezeResolution()
        {
            if (PropertyValueConvertersResolver.HasCurrent == false)
                PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(
                    new ActivatorServiceProvider(), Logger,
                    new[]
                        {
                            typeof(DatePickerValueConverter),
                            typeof(TinyMceValueConverter),
                            typeof(YesNoValueConverter)
                        });    

            PublishedCachesResolver.Current = new PublishedCachesResolver(new PublishedCaches(
                new PublishedContentCache(), new PublishedMediaCache(ApplicationContext)));

            if (PublishedContentModelFactoryResolver.HasCurrent == false)
                PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();

            base.FreezeResolution();
        }

        public override void TearDown()
        {
            base.TearDown();
            
            UmbracoContext.Current = null;
        }
    }
}