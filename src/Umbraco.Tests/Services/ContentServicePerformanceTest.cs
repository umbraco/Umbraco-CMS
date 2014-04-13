using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [TestFixture, NUnit.Framework.Ignore]
    public class ContentServicePerformanceTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
            CreateTestData();
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

            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(unitOfWork);

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

        [Test, NUnit.Framework.Ignore]
        public void Getting_1000_Uncached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(unitOfWork);

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

        [Test]
        public void Getting_100_Cached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 100);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(unitOfWork);

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

        [Test, NUnit.Framework.Ignore]
        public void Getting_1000_Cached_Items()
        {
            // Arrange
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var pages = MockedContent.CreateTextpageContent(contentType, -1, 1000);
            ServiceContext.ContentService.Save(pages, 0);

            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(unitOfWork);

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