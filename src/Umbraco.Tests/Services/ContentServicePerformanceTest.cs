using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, NUnit.Framework.Ignore]
    public class ContentServicePerformanceTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
            CreateTestData();
        }

        protected override void FreezeResolution()
        {
            ProfilerResolver.Current = new ProfilerResolver(new TestProfiler());

            base.FreezeResolution();
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

            var contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "test1");
            var contentType2 = MockedContentTypes.CreateTextpageContentType("test2", "test2");
            var contentType3 = MockedContentTypes.CreateTextpageContentType("test3", "test3");
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
            using (DisposableTimer.TraceDuration<ContentServicePerformanceTest>("Getting all content in site"))
            {
                TestProfiler.Enable();
                total.AddRange(ServiceContext.ContentService.GetRootContent());
                foreach (var content in total.ToArray())
                {
                    total.AddRange(ServiceContext.ContentService.GetDescendants(content));
                }
                TestProfiler.Disable();
                LogHelper.Info<ContentServicePerformanceTest>("Returned " + total.Count + " items");
            }

        }

        [Test]
        public void Creating_100_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);

            // Act
            Stopwatch watch = Stopwatch.StartNew();
            ServiceContext.ContentService.Save(pages, 0);
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;

            Console.WriteLine("100 content items saved in {0} ms", elapsed);

            // Assert
            Assert.That(pages.Any(x => x.HasIdentity == false), Is.False);
        }

        [Test]
        public void Creating_1000_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);

            // Act
            Stopwatch watch = Stopwatch.StartNew();
            ServiceContext.ContentService.Save(pages, 0);
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;

            Console.WriteLine("100 content items saved in {0} ms", elapsed);

            // Assert
            Assert.That(pages.Any(x => x.HasIdentity == false), Is.False);
        }

        [Test]
        public void Getting_100_Uncached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            using (var tRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>()))
            using (var tagRepo = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
            using (var ctRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, tRepository))
            using (var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, ctRepository, tRepository, tagRepo))
            {
                // Act
                Stopwatch watch = Stopwatch.StartNew();
                var contents = repository.GetAll();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Console.WriteLine("100 content items retrieved in {0} ms without caching", elapsed);

                // Assert
                Assert.That(contents.Any(x => x.HasIdentity == false), Is.False);
                Assert.That(contents.Any(x => x == null), Is.False);
            }


        }

        [Test, NUnit.Framework.Ignore]
        public void Getting_1000_Uncached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var tRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>()))
            using (var tagRepo = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
            using (var ctRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, tRepository))
            using (var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, ctRepository, tRepository, tagRepo))
            {
                // Act
                Stopwatch watch = Stopwatch.StartNew();
                var contents = repository.GetAll();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Console.WriteLine("1000 content items retrieved in {0} ms without caching", elapsed);

                // Assert
                //Assert.That(contents.Any(x => x.HasIdentity == false), Is.False);
                //Assert.That(contents.Any(x => x == null), Is.False);
            }
        }

        [Test]
        public void Getting_100_Cached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            using (var tRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>()))
            using (var tagRepo = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
            using (var ctRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, tRepository))
            using (var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, ctRepository, tRepository, tagRepo))
            {

                // Act
                var contents = repository.GetAll();

                Stopwatch watch = Stopwatch.StartNew();
                var contentsCached = repository.GetAll();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Console.WriteLine("100 content items retrieved in {0} ms with caching", elapsed);

                // Assert
                Assert.That(contentsCached.Any(x => x.HasIdentity == false), Is.False);
                Assert.That(contentsCached.Any(x => x == null), Is.False);
                Assert.That(contentsCached.Count(), Is.EqualTo(contents.Count()));
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void Getting_1000_Cached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var tRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>()))
            using (var tagRepo = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
            using (var ctRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, tRepository))
            using (var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, ctRepository, tRepository, tagRepo))
            {

                // Act
                var contents = repository.GetAll();

                Stopwatch watch = Stopwatch.StartNew();
                var contentsCached = repository.GetAll();
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;

                Console.WriteLine("1000 content items retrieved in {0} ms with caching", elapsed);

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
            ContentType contentType = MockedContentTypes.CreateTextpageContentType();
            ServiceContext.ContentTypeService.Save(contentType);
        }
    }
}