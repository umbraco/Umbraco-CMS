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
            
            UmbracoSettings.SettingsFilePath = Core.IO.IOHelper.MapPath(Core.IO.SystemDirectories.Config + Path.DirectorySeparatorChar, false);

            //need to specify a custom callback for unit tests
            PublishedContentHelper.GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
                {
                    if (propertyAlias == "content")
                    {
                        //return the rte type id
                        return Guid.Parse(Constants.PropertyEditors.TinyMCEv3);
                    }
                    return Guid.Empty;
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

            PublishedContentCacheResolver.Current = new PublishedContentCacheResolver(new PublishedContentCache());
            PublishedMediaCacheResolver.Current = new PublishedMediaCacheResolver(new PublishedMediaCache());

            base.FreezeResolution();
        }

        public override void TearDown()
        {
            base.TearDown();
            
            UmbracoContext.Current = null;
        }
    }
}