using System;
using System.IO;
using System.Linq;
using System.Web.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Tests.Common;

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

            Composition.RegisterUnique<IPublishedModelFactory>(f => new PublishedModelFactory(f.GetInstance<TypeLoader>().GetTypes<PublishedContentModel>(), f.GetInstance<IPublishedValueFallback>()));
        }

        protected override TypeLoader CreateTypeLoader(IIOHelper ioHelper, ITypeFinder typeFinder, IAppPolicyCache runtimeCache, ILogger<TypeLoader> logger, IProfilingLogger profilingLogger ,  IHostingEnvironment hostingEnvironment)
        {
            var baseLoader = base.CreateTypeLoader(ioHelper, typeFinder, runtimeCache, logger, profilingLogger , hostingEnvironment);

            return new TypeLoader(typeFinder, runtimeCache, new DirectoryInfo(hostingEnvironment.LocalTempPath), logger, profilingLogger , false,
                // this is so the model factory looks into the test assembly
                baseLoader.AssembliesToScan
                    .Union(new[] {typeof(PublishedContentMoreTests).Assembly})
                    .ToList());
        }

        private IUmbracoContext GetUmbracoContext()
        {
            RouteData routeData = null;

            var publishedSnapshot = CreatePublishedSnapshot();

            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            publishedSnapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedSnapshot);

            var globalSettings = TestObjects.GetGlobalSettings();

            var httpContext = GetHttpContextFactory("http://umbraco.local/", routeData).HttpContext;

            var httpContextAccessor = TestHelper.GetHttpContextAccessor(httpContext);
            var umbracoContext = new UmbracoContext(
                httpContextAccessor,
                publishedSnapshotService.Object,
                Mock.Of<IBackofficeSecurity>(),
                globalSettings,
                HostingEnvironment,
                new TestVariationContextAccessor(),
                UriUtility,
                new AspNetCookieManager(httpContextAccessor));

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
                new DataType(new VoidEditor(NullLoggerFactory.Instance, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(),  Mock.Of<ILocalizedTextService>(), Mock.Of<IShortStringHelper>())) { Id = 1 });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
            var caches = new SolidPublishedSnapshot();
            var cache = caches.InnerContentCache;
            PopulateCache(factory, cache);
            return caches;
        }

        internal abstract void PopulateCache(PublishedContentTypeFactory factory, SolidPublishedContentCache cache);
    }
}
