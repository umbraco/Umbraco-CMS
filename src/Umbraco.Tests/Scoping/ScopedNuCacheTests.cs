using System;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Net;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class ScopedNuCacheTests : TestWithDatabaseBase
    {
        private DistributedCacheBinder _distributedCacheBinder;

        protected override void Compose()
        {
            base.Compose();

            // the cache refresher component needs to trigger to refresh caches
            // but then, it requires a lot of plumbing ;(
            // FIXME: and we cannot inject a DistributedCache yet
            // so doing all this mess
            Builder.Services.AddUnique<IServerMessenger, ScopedXmlTests.LocalServerMessenger>();
            Builder.Services.AddUnique(f => Mock.Of<IServerRegistrar>());
            Builder.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Builder.TypeLoader.GetCacheRefreshers());
        }

        public override void TearDown()
        {
            base.TearDown();

            _distributedCacheBinder?.UnbindEvents();
            _distributedCacheBinder = null;

            _onPublishedAssertAction = null;
            ContentService.Published -= OnPublishedAssert;
        }

        private void OnPublishedAssert(IContentService sender, PublishEventArgs<IContent> args)
        {
            _onPublishedAssertAction?.Invoke();
        }

        private Action _onPublishedAssertAction;

        protected override IPublishedSnapshotService CreatePublishedSnapshotService(GlobalSettings globalSettings = null)
        {
            var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
            var publishedSnapshotAccessor = new UmbracoContextPublishedSnapshotAccessor(Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(() => RuntimeLevel.Run);

            var contentTypeFactory = Factory.GetRequiredService<IPublishedContentTypeFactory>();
            var documentRepository = Mock.Of<IDocumentRepository>();
            var mediaRepository = Mock.Of<IMediaRepository>();
            var memberRepository = Mock.Of<IMemberRepository>();
            var hostingEnvironment = TestHelper.GetHostingEnvironment();

            var typeFinder = TestHelper.GetTypeFinder();

            var nuCacheSettings = new NuCacheSettings();
            var lifetime = new Mock<IUmbracoApplicationLifetime>();
            var snapshotService = new PublishedSnapshotService(
                options,
                null,
                lifetime.Object,
                runtimeStateMock.Object,
                ServiceContext,
                contentTypeFactory,
                publishedSnapshotAccessor,
                Mock.Of<IVariationContextAccessor>(),
                ProfilingLogger,
                NullLoggerFactory.Instance,
                ScopeProvider,
                documentRepository, mediaRepository, memberRepository,
                DefaultCultureAccessor,
                new DatabaseDataSource(Mock.Of<ILogger<DatabaseDataSource>>()),
                Microsoft.Extensions.Options.Options.Create(globalSettings ?? new GlobalSettings()),
                Factory.GetRequiredService<IEntityXmlSerializer>(),
                new NoopPublishedModelFactory(),
                new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider(ShortStringHelper) }),
                hostingEnvironment,
                Mock.Of<IShortStringHelper>(),
                IOHelper,
                Microsoft.Extensions.Options.Options.Create(nuCacheSettings));

            lifetime.Raise(e => e.ApplicationInit += null, EventArgs.Empty);

            return snapshotService;
        }

        protected IUmbracoContext GetUmbracoContextNu(string url, RouteData routeData = null, bool setSingleton = false)
        {
            // ensure we have a PublishedSnapshotService
            var service = PublishedSnapshotService as PublishedSnapshotService;

            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;
            var httpContextAccessor = TestHelper.GetHttpContextAccessor(httpContext);
            var globalSettings = TestObjects.GetGlobalSettings();
            var umbracoContext = new UmbracoContext(
                httpContextAccessor,
                service,
                Mock.Of<IBackOfficeSecurity>(),
                globalSettings,
                HostingEnvironment,
                new TestVariationContextAccessor(),
                UriUtility,
                new AspNetCookieManager(httpContextAccessor));

            if (setSingleton)
                Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;

            return umbracoContext;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestScope(bool complete)
        {
            var umbracoContext = GetUmbracoContextNu("http://example.com/", setSingleton: true);

            // wire cache refresher
            _distributedCacheBinder = new DistributedCacheBinder(new DistributedCache(Current.ServerMessenger, Current.CacheRefreshers), Mock.Of<IUmbracoContextFactory>(), Mock.Of<ILogger<DistributedCacheBinder>>());
            _distributedCacheBinder.BindEvents(true);

            // create document type, document
            var contentType = new ContentType(ShortStringHelper, -1) { Alias = "CustomDocument", Name = "Custom Document" };
            ServiceContext.ContentTypeService.Save(contentType);
            var item = new Content("name", -1, contentType);

            // event handler
            var evented = 0;
            _onPublishedAssertAction = () =>
            {
                evented++;

                var e = umbracoContext.Content.GetById(item.Id);

                // during events, due to LiveSnapshot, we see the changes
                Assert.IsNotNull(e);
                Assert.AreEqual("changed", e.Name(VariationContextAccessor));
            };

            using (var scope = ScopeProvider.CreateScope())
            {
                ServiceContext.ContentService.SaveAndPublish(item);
                scope.Complete();
            }

            // been created
            var x = umbracoContext.Content.GetById(item.Id);
            Assert.IsNotNull(x);
            Assert.AreEqual("name", x.Name(VariationContextAccessor));

            ContentService.Published += OnPublishedAssert;

            using (var scope = ScopeProvider.CreateScope())
            {
                item.Name = "changed";
                ServiceContext.ContentService.SaveAndPublish(item);

                if (complete)
                    scope.Complete();
            }

            // only 1 event occuring because we are publishing twice for the same event for
            // the same object and the scope deduplicates the events (uses the latest)
            Assert.AreEqual(complete ? 1 : 0, evented);

            // after the scope,
            // if completed, we see the changes
            // else changes have been rolled back
            x = umbracoContext.Content.GetById(item.Id);
            Assert.IsNotNull(x);
            Assert.AreEqual(complete ? "changed" : "name", x.Name(VariationContextAccessor));
        }
    }
}
