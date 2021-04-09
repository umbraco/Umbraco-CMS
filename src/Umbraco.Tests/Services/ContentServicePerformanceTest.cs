using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Services
{
    [TestFixture, NUnit.Framework.Ignore("fixme - ignored test")]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentServicePerformanceTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();
            CreateTestData();
        }

        protected override void Compose()
        {
            base.Compose();
            Composition.Register<IProfiler, TestProfiler>();
        }

        private DocumentRepository CreateDocumentRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor)provider;
            var tRepository = new TemplateRepository(accessor, AppCaches.Disabled, Logger, TestObjects.GetFileSystemsMock());
            var tagRepo = new TagRepository(accessor, AppCaches.Disabled, Logger);
            var commonRepository = new ContentTypeCommonRepository(accessor, tRepository, AppCaches);
            var languageRepository = new LanguageRepository(accessor, AppCaches.Disabled, Logger);
            var ctRepository = new ContentTypeRepository(accessor, AppCaches.Disabled, Logger, commonRepository, languageRepository);
            var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Logger);
            var entityRepository = new EntityRepository(accessor);
            var relationRepository = new RelationRepository(accessor, Logger, relationTypeRepository, entityRepository);
            var propertyEditors = new Lazy<PropertyEditorCollection>(() => new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>())));
            var dataValueReferences = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new DocumentRepository(accessor, AppCaches.Disabled, Logger, ctRepository, tRepository, tagRepo, languageRepository, relationRepository, relationTypeRepository, propertyEditors, dataValueReferences);
            return repository;
        }

        [Test]
        public void Profiler()
        {
            Assert.IsInstanceOf<TestProfiler>(Current.Profiler);
        }

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var logger = new DebugDiagnosticsLogger();
            var profiler = new TestProfiler();
            return new ProfilingLogger(logger, profiler);
        }

        [Test]
        public void Retrieving_All_Content_In_Site()
        {
            //NOTE: Doing this the old 1 by 1 way and based on the results of the ContentServicePerformanceTest.Retrieving_All_Content_In_Site
            // the old way takes 143795ms, the new above way takes:
            // 14249ms
            //
            // ... NOPE, made some new changes, it is now....
            // 5290ms  !!!!!!
            //
            // that is a 96% savings of processing and sql calls!
            //
            // ... NOPE, made even more nice changes, it is now...
            // 4452ms !!!!!!!

            var contentType1 = MockedContentTypes.CreateTextPageContentType("test1", "test1");
            var contentType2 = MockedContentTypes.CreateTextPageContentType("test2", "test2");
            var contentType3 = MockedContentTypes.CreateTextPageContentType("test3", "test3");
            ServiceContext.ContentTypeService.Save(new[] { contentType1, contentType2, contentType3 });
            contentType1.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => contentType2.Id), 0, contentType2.Alias),
                new ContentTypeSort(new Lazy<int>(() => contentType3.Id), 1, contentType3.Alias)
            };
            contentType2.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias),
                new ContentTypeSort(new Lazy<int>(() => contentType3.Id), 1, contentType3.Alias)
            };
            contentType3.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias),
                new ContentTypeSort(new Lazy<int>(() => contentType2.Id), 1, contentType2.Alias)
            };
            ServiceContext.ContentTypeService.Save(new[] { contentType1, contentType2, contentType3 });

            var roots = MockedContent.CreateTextpageContent(contentType1, -1, 10);
            ServiceContext.ContentService.Save(roots);
            foreach (var root in roots)
            {
                var item1 = MockedContent.CreateTextpageContent(contentType1, root.Id, 10);
                var item2 = MockedContent.CreateTextpageContent(contentType2, root.Id, 10);
                var item3 = MockedContent.CreateTextpageContent(contentType3, root.Id, 10);

                ServiceContext.ContentService.Save(item1.Concat(item2).Concat(item3));
            }

            var total = new List<IContent>();

            using (GetTestProfilingLogger().TraceDuration<ContentServicePerformanceTest>("Getting all content in site"))
            {
                TestProfiler.Enable();
                total.AddRange(ServiceContext.ContentService.GetRootContent());
                foreach (var content in total.ToArray())
                {
                    total.AddRange(ServiceContext.ContentService.GetPagedDescendants(content.Id, 0, int.MaxValue, out var _));
                }
                TestProfiler.Disable();
                Current.Logger.Info<ContentServicePerformanceTest,int>("Returned {Total} items", total.Count);
            }
        }

        [Test]
        public void Creating_100_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);

            // Act
            Stopwatch watch = Stopwatch.StartNew();
            ServiceContext.ContentService.Save(pages, 0);
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;

            Debug.Print("100 content items saved in {0} ms", elapsed);

            // Assert
            Assert.That(pages.Any(x => x.HasIdentity == false), Is.False);
        }

        [Test]
        public void Creating_1000_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);

            // Act
            Stopwatch watch = Stopwatch.StartNew();
            ServiceContext.ContentService.Save(pages, 0);
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;

            Debug.Print("100 content items saved in {0} ms", elapsed);

            // Assert
            Assert.That(pages.Any(x => x.HasIdentity == false), Is.False);
        }

        [Test]
        public void Getting_100_Uncached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateDocumentRepository(provider);

                // Act
                Stopwatch watch = Stopwatch.StartNew();
                var contents = repository.GetMany();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Debug.Print("100 content items retrieved in {0} ms without caching", elapsed);

                // Assert
                Assert.That(contents.Any(x => x.HasIdentity == false), Is.False);
                Assert.That(contents.Any(x => x == null), Is.False);
            }


        }

        [Test, NUnit.Framework.Ignore("fixme - ignored test")]
        public void Getting_1000_Uncached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateDocumentRepository(provider);

                // Act
                Stopwatch watch = Stopwatch.StartNew();
                var contents = repository.GetMany();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Debug.Print("1000 content items retrieved in {0} ms without caching", elapsed);

                // Assert
                //Assert.That(contents.Any(x => x.HasIdentity == false), Is.False);
                //Assert.That(contents.Any(x => x == null), Is.False);
            }
        }

        [Test]
        public void Getting_100_Cached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateDocumentRepository(provider);

                // Act
                var contents = repository.GetMany();

                Stopwatch watch = Stopwatch.StartNew();
                var contentsCached = repository.GetMany();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Debug.Print("100 content items retrieved in {0} ms with caching", elapsed);

                // Assert
                Assert.That(contentsCached.Any(x => x.HasIdentity == false), Is.False);
                Assert.That(contentsCached.Any(x => x == null), Is.False);
                Assert.That(contentsCached.Count(), Is.EqualTo(contents.Count()));
            }
        }

        [Test, NUnit.Framework.Ignore("fixme - ignored test")]
        public void Getting_1000_Cached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateDocumentRepository(provider);

                // Act
                var contents = repository.GetMany();

                Stopwatch watch = Stopwatch.StartNew();
                var contentsCached = repository.GetMany();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Debug.Print("1000 content items retrieved in {0} ms with caching", elapsed);

                // Assert
                //Assert.That(contentsCached.Any(x => x.HasIdentity == false), Is.False);
                //Assert.That(contentsCached.Any(x => x == null), Is.False);
                //Assert.That(contentsCached.Count(), Is.EqualTo(contents.Count()));
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            //Create and Save ContentType "textpage" -> NodeDto.NodeIdSeed
            ContentType contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.ContentTypeService.Save(contentType);
        }
    }
}
