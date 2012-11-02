using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class ContentRepositoryTest : BaseDatabaseFactoryTest
    {
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
    }
}