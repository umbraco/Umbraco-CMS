using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Web.Routing;
using Moq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Core.Composing;
using Current = Umbraco.Core.Composing.Current;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;

namespace Umbraco.Tests.PublishedContent
{
    public abstract class PublishedContentSnapshotTestBase : PublishedContentTestBase
    {
        // read http://stackoverflow.com/questions/7713326/extension-method-that-works-on-ienumerablet-and-iqueryablet
        // and http://msmvps.com/blogs/jon_skeet/archive/2010/10/28/overloading-and-generic-constraints.aspx
        // and http://blogs.msdn.com/b/ericlippert/archive/2009/12/10/constraints-are-not-part-of-the-signature.aspx

        public override void SetUp()
        {
            base.SetUp();

            var umbracoContext = GetUmbracoContext();
            Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
        }

        protected override void Compose()
        {
            base.Compose();

            Container.RegisterSingleton<IPublishedModelFactory>(f => new PublishedModelFactory(f.GetInstance<TypeLoader>().GetTypes<PublishedContentModel>()));
        }

        protected override TypeLoader CreateTypeLoader(IRuntimeCacheProvider runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger)
        {
            var pluginManager = base.CreateTypeLoader(runtimeCache, globalSettings, logger);

            // this is so the model factory looks into the test assembly
            pluginManager.AssembliesToScan = pluginManager.AssembliesToScan
                .Union(new[] { typeof (PublishedContentMoreTests).Assembly })
                .ToList();

            return pluginManager;
        }

        private UmbracoContext GetUmbracoContext()
        {
            RouteData routeData = null;

            var publishedSnapshot = CreatePublishedSnapshot();

            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            publishedSnapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedSnapshot);

            var globalSettings = TestObjects.GetGlobalSettings();

            var httpContext = GetHttpContextFactory("http://umbraco.local/", routeData).HttpContext;
            var umbracoContext = new UmbracoContext(
                httpContext,
                publishedSnapshotService.Object,
                new WebSecurity(httpContext, Current.Services.UserService, globalSettings),
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor());

            return umbracoContext;
        }

        public override void TearDown()
        {
            base.TearDown();

            Current.Reset();
        }

        private SolidPublishedSnapshot CreatePublishedSnapshot()
        {
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
            var caches = new SolidPublishedSnapshot();
            var cache = caches.InnerContentCache;
            PopulateCache(factory, cache);
            return caches;
        }

        internal abstract void PopulateCache(PublishedContentTypeFactory factory, SolidPublishedContentCache cache);
    }
}
