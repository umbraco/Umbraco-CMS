using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

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
            Composition.RegisterUnique<IServerMessenger, ScopedXmlTests.LocalServerMessenger>();
            Composition.RegisterUnique(f => Mock.Of<IServerRegistrar>());
            Composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Composition.TypeLoader.GetCacheRefreshers());
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

        protected override IPublishedSnapshotService CreatePublishedSnapshotService()
        {
            var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
            var publishedSnapshotAccessor = new UmbracoContextPublishedSnapshotAccessor(Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(() => RuntimeLevel.Run);

            var contentTypeFactory = Factory.GetInstance<IPublishedContentTypeFactory>();
            var documentRepository = Mock.Of<IDocumentRepository>();
            var mediaRepository = Mock.Of<IMediaRepository>();
            var memberRepository = Mock.Of<IMemberRepository>();

            var nestedContentDataSerializerFactory = new JsonContentNestedDataSerializerFactory();
            return new PublishedSnapshotService(
                options,
                null,
                runtimeStateMock.Object,
                ServiceContext,
                contentTypeFactory,
                null,
                publishedSnapshotAccessor,
                Mock.Of<IVariationContextAccessor>(),
                ProfilingLogger,
                ScopeProvider,
                documentRepository, mediaRepository, memberRepository,
                DefaultCultureAccessor,
                new DatabaseDataSource(nestedContentDataSerializerFactory),
                Factory.GetInstance<IGlobalSettings>(),
                Factory.GetInstance<IEntityXmlSerializer>(),
                Mock.Of<IPublishedModelFactory>(),
                new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider() }),
                new TestSyncBootStateAccessor(SyncBootState.WarmBoot),
                nestedContentDataSerializerFactory);
        }

        protected UmbracoContext GetUmbracoContextNu(string url, int templateId = 1234, RouteData routeData = null, bool setSingleton = false, IUmbracoSettingsSection umbracoSettings = null, IEnumerable<IUrlProvider> urlProviders = null)
        {
            // ensure we have a PublishedSnapshotService
            var service = PublishedSnapshotService as PublishedSnapshotService;

            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;

            var globalSettings = TestObjects.GetGlobalSettings();
            var umbracoContext = new UmbracoContext(
                httpContext,
                service,
                new WebSecurity(httpContext, Current.Services.UserService, globalSettings),
                umbracoSettings ?? SettingsForTests.GetDefaultUmbracoSettings(),
                urlProviders ?? Enumerable.Empty<IUrlProvider>(),
                Enumerable.Empty<IMediaUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor());

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
            _distributedCacheBinder = new DistributedCacheBinder(new DistributedCache(), Mock.Of<IUmbracoContextFactory>(), Mock.Of<ILogger>());
            _distributedCacheBinder.BindEvents(true);

            // create document type, document
            var contentType = new ContentType(-1) { Alias = "CustomDocument", Name = "Custom Document" };
            Current.Services.ContentTypeService.Save(contentType);
            var item = new Content("name", -1, contentType);

            // event handler
            var evented = 0;
            _onPublishedAssertAction = () =>
            {
                evented++;

                var e = umbracoContext.Content.GetById(item.Id);

                // during events, due to LiveSnapshot, we see the changes
                Assert.IsNotNull(e);
                Assert.AreEqual("changed", e.Name());
            };

            using (var scope = ScopeProvider.CreateScope())
            {
                Current.Services.ContentService.SaveAndPublish(item);
                scope.Complete();
            }

            // been created
            var x = umbracoContext.Content.GetById(item.Id);
            Assert.IsNotNull(x);
            Assert.AreEqual("name", x.Name());

            ContentService.Published += OnPublishedAssert;

            using (var scope = ScopeProvider.CreateScope())
            {
                item.Name = "changed";
                Current.Services.ContentService.SaveAndPublish(item);

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
            Assert.AreEqual(complete ? "changed" : "name", x.Name());
        }
    }
}
