using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class ContentRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, contentTypeRepository);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            Content textpage = MockedContent.CreateSimpleContent(contentType);

            // Act
            contentTypeRepository.AddOrUpdate(contentType);
            repository.AddOrUpdate(textpage);
            unitOfWork.Commit();

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(textpage.HasIdentity, Is.True);
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            Content textpage = MockedContent.CreateSimpleContent(contentType);
            
            // Act
            contentTypeRepository.AddOrUpdate(contentType);
            repository.AddOrUpdate(textpage);
            unitOfWork.Commit();
            
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
            repository.AddOrUpdate(subpage);
            unitOfWork.Commit();

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(textpage.HasIdentity, Is.True);
            Assert.That(subpage.HasIdentity, Is.True);
            Assert.That(textpage.Id, Is.EqualTo(subpage.ParentId));
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_ContentRepository_With_RepositoryResolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(unitOfWork);
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            Content textpage = MockedContent.CreateSimpleContent(contentType);

            // Act
            contentTypeRepository.AddOrUpdate(contentType);
            repository.AddOrUpdate(textpage);
            unitOfWork.Commit();

            var repository2 = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
            repository2.AddOrUpdate(subpage);
            unitOfWork.Commit();

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(textpage.HasIdentity, Is.True);
            Assert.That(subpage.HasIdentity, Is.True);
            Assert.That(textpage.Id, Is.EqualTo(subpage.ParentId));
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var content = repository.Get(1048);
            bool dirty = ((Content) content).IsDirty();

            // Assert
            Assert.That(dirty, Is.False);
        }

        [Test]
        public void Can_Perform_Update_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var content = repository.Get(1047);
            content.Name = "About 2";
            repository.AddOrUpdate(content);
            unitOfWork.Commit();
            var updatedContent = repository.Get(1047);

            // Assert
            Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
            Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
        }

        [Test]
        public void Can_Perform_Delete_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            var contentType = contentTypeRepository.Get(1045);
            var content = new Content(1048, contentType);
            content.Name = "Textpage 2 Child Node";
            content.Creator = new Profile(0, "Administrator");
            content.Writer = new Profile(0, "Administrator");

            // Act
            repository.AddOrUpdate(content);
            unitOfWork.Commit();
            var id = content.Id;

            var contentTypeRepository2 = new ContentTypeRepository(unitOfWork);
            var repository2 = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository2);
            repository2.Delete(content);
            unitOfWork.Commit();

            var content1 = repository2.Get(id);

            // Assert
            Assert.That(content1, Is.Null);
        }

        [Test]
        public void Can_Perform_Get_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var content = repository.Get(1048);

            // Assert
            Assert.That(content.Id, Is.EqualTo(1048));
            Assert.That(content.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(content.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(content.ParentId, Is.Not.EqualTo(0));
            Assert.That(content.Name, Is.EqualTo("Text Page 2"));
            Assert.That(content.SortOrder, Is.EqualTo(1));
            Assert.That(content.Version, Is.Not.EqualTo(Guid.Empty));
            Assert.That(content.ContentTypeId, Is.EqualTo(1045));
            Assert.That(content.Path, Is.Not.Empty);
            Assert.That(content.Properties.Any(), Is.True);
        }

        [Test]
        public void Can_Perform_GetByQuery_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var query = Query<IContent>.Builder.Where(x => x.Level == 2);
            var result = repository.GetByQuery(query);

            // Assert
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var contents = repository.GetAll(1047, 1048);

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_Perform_GetAll_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var contents = repository.GetAll();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void Can_Perform_Exists_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            var exists = repository.Exists(1046);

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public void Can_Perform_Count_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var contentTypeRepository = new ContentTypeRepository(unitOfWork);
            var repository = new ContentRepository(unitOfWork, InMemoryCacheProvider.Current, contentTypeRepository);

            // Act
            int level = 2;
            var query = Query<IContent>.Builder.Where(x => x.Level == level);
            var result = repository.Count(query);

            // Assert
            Assert.That(result, Is.GreaterThanOrEqualTo(2));
        }

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> 1045
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.ContentTypeService.Save(contentType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> 1046
            Content textpage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(textpage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> 1047
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
            ServiceContext.ContentService.Save(subpage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> 1048
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
            ServiceContext.ContentService.Save(subpage2, 0);

            //Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1049
            Content trashed = MockedContent.CreateSimpleContent(contentType, "Text Page Deleted", -20);
            trashed.Trashed = true;
            ServiceContext.ContentService.Save(trashed, 0);
        }
    }
}