using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, FacadeServiceRepositoryEvents = true)]
    public class ScopedNuCacheTests : TestWithDatabaseBase
    {
        private CacheRefresherComponent _cacheRefresher;

        protected override void Compose()
        {
            base.Compose();

            // the cache refresher component needs to trigger to refresh caches
            // but then, it requires a lot of plumbing ;(
            // fixme - and we cannot inject a DistributedCache yet
            // so doing all this mess
            Container.RegisterSingleton<IServerMessenger, ScopedXmlTests.LocalServerMessenger>();
            Container.RegisterSingleton(f => Mock.Of<IServerRegistrar>());
            Container.RegisterCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(f => f.TryGetInstance<TypeLoader>().GetCacheRefreshers());
        }

        public override void TearDown()
        {
            base.TearDown();

            _cacheRefresher?.Unbind();
            _cacheRefresher = null;

            //_onPublishedAssertAction = null;
            //ContentService.Published -= OnPublishedAssert;
        }

        protected override IFacadeService CreateFacadeService()
        {
            var options = new FacadeService.Options { IgnoreLocalDb = true };
            var facadeAccessor = new UmbracoContextFacadeAccessor(Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(() => RuntimeLevel.Run);

            return new FacadeService(
                options,
                null,
                runtimeStateMock.Object,
                ServiceContext,
                UowProvider,
                facadeAccessor,
                Logger,
                ScopeProvider);
        }

        protected UmbracoContext GetUmbracoContextNu(string url, int templateId = 1234, RouteData routeData = null, bool setSingleton = false, IUmbracoSettingsSection umbracoSettings = null, IEnumerable<IUrlProvider> urlProviders = null)
        {
            // ensure we have a FacadeService
            var service = FacadeService as FacadeService;

            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;

            var umbracoContext = new UmbracoContext(
                httpContext,
                service,
                new WebSecurity(httpContext, Current.Services.UserService),
                umbracoSettings ?? SettingsForTests.GetDefault(),
                urlProviders ?? Enumerable.Empty<IUrlProvider>());

            if (setSingleton)
                Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;

            return umbracoContext;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WipTest(bool complete)
        {
            var umbracoContext = GetUmbracoContextNu("http://example.com/", setSingleton: true);

            // wire cache refresher
            _cacheRefresher = new CacheRefresherComponent(true);
            _cacheRefresher.Initialize(new DistributedCache());

            // create document type, document
            var contentType = new ContentType(-1) { Alias = "CustomDocument", Name = "Custom Document" };
            Current.Services.ContentTypeService.Save(contentType);
            var item = new Content("name", -1, contentType);

            using (var scope = ScopeProvider.CreateScope())
            {
                Current.Services.ContentService.SaveAndPublishWithStatus(item);
                item.Name = "changed";
                Current.Services.ContentService.SaveAndPublishWithStatus(item);

                if (complete)
                    scope.Complete();
            }

            // fixme - some exceptions are badly swallowed by the scope 'robust exit'?
            // fixme - the plumbing of 'other' content types is badly borked

            var x = umbracoContext.ContentCache.GetById(item.Id);

            if (complete)
            {
                Assert.IsNotNull(x);
                Assert.AreEqual("changed", x.Name);
            }
            else
            {
                Assert.IsNull(x);
            }

            // fixme - should do more tests & ensure it's all consistent even after rollback
        }
    }
}
