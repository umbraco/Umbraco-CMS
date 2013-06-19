using System;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
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
            
            UmbracoSettings.SettingsFilePath = Core.IO.IOHelper.MapPath(Core.IO.SystemDirectories.Config + Path.DirectorySeparatorChar, false);

            PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
                new[]
                    {
                        typeof(DatePickerPropertyEditorValueConverter),
                        typeof(TinyMcePropertyEditorValueConverter),
                        typeof(YesNoPropertyEditorValueConverter)
                    });            

            //need to specify a custom callback for unit tests
            PublishedContentHelper.GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
                {
                    if (propertyAlias.InvariantEquals("content"))
                    {
                        //return the rte type id
                        return Guid.Parse("5e9b75ae-face-41c8-b47e-5f4b0fd82f83");
                    }
                    return Guid.Empty;
                };

            var rCtx = GetRoutingContext("/test", 1234);
            UmbracoContext.Current = rCtx.UmbracoContext;
            PublishedContentStoreResolver.Current = new PublishedContentStoreResolver(new DefaultPublishedContentStore());
            PublishedMediaStoreResolver.Current = new PublishedMediaStoreResolver(new DefaultPublishedMediaStore());
        }

        public override void TearDown()
        {
            base.TearDown();
            
            PropertyEditorValueConvertersResolver.Reset();
            PublishedContentStoreResolver.Reset();
            PublishedMediaStoreResolver.Reset();
            UmbracoContext.Current = null;
        }
    }
}