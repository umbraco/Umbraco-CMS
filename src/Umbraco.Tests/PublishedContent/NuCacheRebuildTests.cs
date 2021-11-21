using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Tests.Testing;
using Umbraco.Web.PropertyEditors;

using Current = Umbraco.Web.Composing.Current;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using System;
using Umbraco.Core.Cache;
using Umbraco.Web.Cache;
using Umbraco.Core.Sync;
using static Umbraco.Tests.Cache.DistributedCache.DistributedCacheTests;
using static Umbraco.Tests.Integration.ContentEventsTests;
using Umbraco.Tests.Services;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Configuration;
using Umbraco.Web.PublishedCache.NuCache.DataSource;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true, WithApplication = true)]
    public class NuCacheRebuildTests: TestWithDatabaseBase
    {
        private IContentTypeService _contentTypeService;
        private IContentType _contentType;

        protected override void Initialize()
        {
            base.Initialize();

            if (!(PublishedSnapshotService is PublishedSnapshotService))
            {
                var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
                var runtime = Mock.Of<IRuntimeState>();
                PublishedSnapshotService = new PublishedSnapshotService(
                    options,
                    null,
                    runtime,
                    ServiceContext,
                    Factory.GetInstance<IPublishedContentTypeFactory>(),
                    null,
                    new TestPublishedSnapshotAccessor(),
                    new TestVariationContextAccessor(),
                    Mock.Of<IProfilingLogger>(),
                    ScopeProvider,
                    Factory.GetInstance<IDocumentRepository>(), Factory.GetInstance<IMediaRepository>(), Factory.GetInstance<IMemberRepository>(),
                    DefaultCultureAccessor,
                    new DatabaseDataSource(new JsonContentNestedDataSerializerFactory()),
                    new GlobalSettings(),
                    Factory.GetInstance<IEntityXmlSerializer>(),
                    Factory.GetInstance<IPublishedModelFactory>(),
                    new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider() }),
                    new TestSyncBootStateAccessor(SyncBootState.WarmBoot),
                    new JsonContentNestedDataSerializerFactory()
                    );
            }
        }
        public override void SetUp()
        {
            base.SetUp();
            ContentRepositoryBase.ThrowOnWarning = true;
        }

        public override void TearDown()
        {
            ContentRepositoryBase.ThrowOnWarning = false;
            base.TearDown();
        }

        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterUnique(factory => Mock.Of<ILocalizedTextService>());
        }
        [Test]
        public void UnpublishedNameChanges()
        {
            var urlSegmentProvider = new DefaultUrlSegmentProvider();

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "hello", Constants.System.Root);

            ServiceContext.ContentService.SaveAndPublish(content);
            var cachedContent = ServiceContext.ContentService.GetById(content.Id);
            var segment = urlSegmentProvider.GetUrlSegment(cachedContent);

            // Does a new node work?

            Assert.AreEqual("hello", segment);

            content.Name = "goodbye";
            cachedContent = ServiceContext.ContentService.GetById(content.Id);
            segment = urlSegmentProvider.GetUrlSegment(cachedContent);

            // We didn't save anything, so all should still be the same

            Assert.AreEqual("hello", segment);

            ServiceContext.ContentService.Save(content);
            cachedContent = ServiceContext.ContentService.GetById(content.Id);
            segment = urlSegmentProvider.GetUrlSegment(cachedContent);

            // At this point we have saved the new name, but not published. The url should still be the previous name

            Assert.AreEqual("hello", segment);

            PublishedSnapshotService.Rebuild();

            cachedContent = ServiceContext.ContentService.GetById(content.Id);
            segment = urlSegmentProvider.GetUrlSegment(cachedContent);

            // After a rebuild, the unpublished name should still not be the url.
            // This was previously incorrect, per #11074

            Assert.AreEqual("hello", segment);

            ServiceContext.ContentService.SaveAndPublish(content);
            cachedContent = ServiceContext.ContentService.GetById(content.Id);
            segment = urlSegmentProvider.GetUrlSegment(cachedContent);

            // The page has now been published, so we should see the new url segment
            Assert.AreEqual("goodbye", segment);

            PublishedSnapshotService.Rebuild();
            cachedContent = ServiceContext.ContentService.GetById(content.Id);
            segment = urlSegmentProvider.GetUrlSegment(cachedContent);

            // Just double checking that things remain after a rebuild
            Assert.AreEqual("goodbye", segment);

        }

    }
}
