using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class MediaRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {           
            base.Initialize();

            CreateTestData();
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var mediaTypeRepository = RepositoryResolver.Current.ResolveByType<IMediaTypeRepository>(unitOfWork);
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            var mediaType = mediaTypeRepository.Get(1032);
            var image = MockedMedia.CreateMediaImage(mediaType, -1);

            // Act
            mediaTypeRepository.AddOrUpdate(mediaType);
            repository.AddOrUpdate(image);
            unitOfWork.Commit();

            // Assert
            Assert.That(mediaType.HasIdentity, Is.True);
            Assert.That(image.HasIdentity, Is.True);
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var mediaTypeRepository = RepositoryResolver.Current.ResolveByType<IMediaTypeRepository>(unitOfWork);
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            var mediaType = mediaTypeRepository.Get(1032);
            var file = MockedMedia.CreateMediaFile(mediaType, -1);

            // Act
            repository.AddOrUpdate(file);
            unitOfWork.Commit();

            var image = MockedMedia.CreateMediaImage(mediaType, -1);
            repository.AddOrUpdate(image);
            unitOfWork.Commit();

            // Assert
            Assert.That(file.HasIdentity, Is.True);
            Assert.That(image.HasIdentity, Is.True);
            Assert.That(file.Name, Is.EqualTo("Test File"));
            Assert.That(image.Name, Is.EqualTo("Test Image"));
            Assert.That(file.ContentTypeId, Is.EqualTo(mediaType.Id));
            Assert.That(image.ContentTypeId, Is.EqualTo(mediaType.Id));
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_MediaRepository_With_RepositoryResolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var mediaTypeRepository = RepositoryResolver.Current.ResolveByType<IMediaTypeRepository>(unitOfWork);
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            var mediaType = mediaTypeRepository.Get(1032);
            var file = MockedMedia.CreateMediaFile(mediaType, -1);

            // Act
            repository.AddOrUpdate(file);
            unitOfWork.Commit();

            var image = MockedMedia.CreateMediaImage(mediaType, -1);
            repository.AddOrUpdate(image);
            unitOfWork.Commit();

            // Assert
            Assert.That(file.HasIdentity, Is.True);
            Assert.That(image.HasIdentity, Is.True);
            Assert.That(file.Name, Is.EqualTo("Test File"));
            Assert.That(image.Name, Is.EqualTo("Test Image"));
            Assert.That(file.ContentTypeId, Is.EqualTo(mediaType.Id));
            Assert.That(image.ContentTypeId, Is.EqualTo(mediaType.Id));
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var media = repository.Get(1046);
            bool dirty = ((ICanBeDirty)media).IsDirty();

            // Assert
            Assert.That(dirty, Is.False);
        }

        [Test]
        public void Can_Perform_Update_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var content = repository.Get(1047);
            content.Name = "Test File Updated";
            repository.AddOrUpdate(content);
            unitOfWork.Commit();

            var updatedContent = repository.Get(1047);

            // Assert
            Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
            Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
        }

        [Test]
        public void Can_Perform_Delete_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var media = repository.Get(1047);
            repository.Delete(media);
            unitOfWork.Commit();

            var deleted = repository.Get(1047);
            var exists = repository.Exists(1047);

            // Assert
            Assert.That(deleted, Is.Null);
            Assert.That(exists, Is.False);
        }

        [Test]
        public void Can_Perform_Get_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var media = repository.Get(1046);

            // Assert
            Assert.That(media.Id, Is.EqualTo(1046));
            Assert.That(media.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(media.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(media.ParentId, Is.Not.EqualTo(0));
            Assert.That(media.Name, Is.EqualTo("Test Image"));
            Assert.That(media.SortOrder, Is.EqualTo(0));
            Assert.That(media.Version, Is.Not.EqualTo(Guid.Empty));
            Assert.That(media.ContentTypeId, Is.EqualTo(1032));
            Assert.That(media.Path, Is.Not.Empty);
            Assert.That(media.Properties.Any(), Is.True);
        }

        [Test]
        public void Can_Perform_GetByQuery_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var query = Query<IMedia>.Builder.Where(x => x.Level == 2);
            var result = repository.GetByQuery(query);

            // Assert
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2));//There should be two entities on level 2: File and Media
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var medias = repository.GetAll(1046, 1047);

            // Assert
            Assert.That(medias, Is.Not.Null);
            Assert.That(medias.Any(), Is.True);
            Assert.That(medias.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_Perform_GetAll_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var medias = repository.GetAll();

            // Assert
            Assert.That(medias, Is.Not.Null);
            Assert.That(medias.Any(), Is.True);
            Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void Can_Perform_Exists_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            var exists = repository.Exists(1046);
            var existsToo = repository.Exists(1046);
            var doesntExists = repository.Exists(1050);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(existsToo, Is.True);
            Assert.That(doesntExists, Is.False);
        }

        [Test]
        public void Can_Perform_Count_On_MediaRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IMediaRepository>(unitOfWork);

            // Act
            int level = 2;
            var query = Query<IMedia>.Builder.Where(x => x.Level == level);
            var result = repository.Count(query);

            // Assert
            Assert.That(result, Is.GreaterThanOrEqualTo(2));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            //Create and Save folder-Media -> 1045
            var folderMediaType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
            ServiceContext.MediaService.Save(folder, 0);

            //Create and Save image-Media -> 1046
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);
            var image = MockedMedia.CreateMediaImage(imageMediaType, folder.Id);
            ServiceContext.MediaService.Save(image, 0);

            //Create and Save file-Media -> 1047
            var fileMediaType = ServiceContext.ContentTypeService.GetMediaType(1033);
            var file = MockedMedia.CreateMediaFile(fileMediaType, folder.Id);
            ServiceContext.MediaService.Save(file, 0);
        }
    }
}