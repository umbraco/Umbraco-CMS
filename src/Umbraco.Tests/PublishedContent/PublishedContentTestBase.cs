using System;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
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
            
            //need to specify a custom callback for unit tests
            PublishedContentHelper.GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
                {
                    if (propertyAlias.InvariantEquals("content"))
                    {
                        //return the rte type id
                        return Constants.PropertyEditors.TinyMCEv3Alias;
                    }
                    return string.Empty;
                };

            var rCtx = GetRoutingContext("/test", 1234);
            UmbracoContext.Current = rCtx.UmbracoContext;
            
        }

        protected override void FreezeResolution()
        {
            PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
                new[]
                    {
                        typeof(DatePickerPropertyEditorValueConverter),
                        typeof(TinyMcePropertyEditorValueConverter),
                        typeof(YesNoPropertyEditorValueConverter)
                    });    

            PublishedCachesResolver.Current = new PublishedCachesResolver(new PublishedCaches(
                new PublishedContentCache(), new PublishedMediaCache()));

            base.FreezeResolution();
        }

        public override void TearDown()
        {
            base.TearDown();
            
            UmbracoContext.Current = null;
        }
    }
}