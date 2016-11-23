using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MediaRepositoryTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            CreateTestData();
        }

        private MediaRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out MediaTypeRepository mediaTypeRepository)
        {
            mediaTypeRepository = new MediaTypeRepository(unitOfWork, CacheHelper, Logger, QueryFactory);
            var tagRepository = new TagRepository(unitOfWork, CacheHelper, Logger, QueryFactory);
            var repository = new MediaRepository(unitOfWork, CacheHelper, Logger, mediaTypeRepository, tagRepository, Mock.Of<IContentSection>(), QueryFactory);
            return repository;
        }

        [Test]
        public void Can_Perform_Add_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                var mediaType = mediaTypeRepository.Get(1032);
                var image = MockedMedia.CreateMediaImage(mediaType, -1);

                // Act
                mediaTypeRepository.AddOrUpdate(mediaType);
                repository.AddOrUpdate(image);
                unitOfWork.Flush();

                var fetched = repository.Get(image.Id);

                // Assert
                Assert.That(mediaType.HasIdentity, Is.True);
                Assert.That(image.HasIdentity, Is.True);

                TestHelper.AssertAllPropertyValuesAreEquals(image, fetched, "yyyy-MM-dd HH:mm:ss");
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                var mediaType = mediaTypeRepository.Get(1032);
                var file = MockedMedia.CreateMediaFile(mediaType, -1);

                // Act
                repository.AddOrUpdate(file);
                unitOfWork.Flush();

                var image = MockedMedia.CreateMediaImage(mediaType, -1);
                repository.AddOrUpdate(image);
                unitOfWork.Flush();

                // Assert
                Assert.That(file.HasIdentity, Is.True);
                Assert.That(image.HasIdentity, Is.True);
                Assert.That(file.Name, Is.EqualTo("Test File"));
                Assert.That(image.Name, Is.EqualTo("Test Image"));
                Assert.That(file.ContentTypeId, Is.EqualTo(mediaType.Id));
                Assert.That(image.ContentTypeId, Is.EqualTo(mediaType.Id));
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_MediaRepository_With_RepositoryResolver()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                var mediaType = mediaTypeRepository.Get(1032);
                var file = MockedMedia.CreateMediaFile(mediaType, -1);

                // Act
                repository.AddOrUpdate(file);
                unitOfWork.Flush();

                var image = MockedMedia.CreateMediaImage(mediaType, -1);
                repository.AddOrUpdate(image);
                unitOfWork.Flush();

                // Assert
                Assert.That(file.HasIdentity, Is.True);
                Assert.That(image.HasIdentity, Is.True);
                Assert.That(file.Name, Is.EqualTo("Test File"));
                Assert.That(image.Name, Is.EqualTo("Test Image"));
                Assert.That(file.ContentTypeId, Is.EqualTo(mediaType.Id));
                Assert.That(image.ContentTypeId, Is.EqualTo(mediaType.Id));
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var media = repository.Get(NodeDto.NodeIdSeed + 1);
                bool dirty = ((ICanBeDirty)media).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Name = "Test File Updated";
                repository.AddOrUpdate(content);
                unitOfWork.Flush();

                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
                Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var media = repository.Get(NodeDto.NodeIdSeed + 2);
                repository.Delete(media);
                unitOfWork.Flush();

                var deleted = repository.Get(NodeDto.NodeIdSeed + 2);
                var exists = repository.Exists(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(deleted, Is.Null);
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var media = repository.Get(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(media.Id, Is.EqualTo(NodeDto.NodeIdSeed + 1));
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
        }

        [Test]
        public void Can_Perform_GetByQuery_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2)); //There should be two entities on level 2: File and Media
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_ForFirstPage_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "SortOrder", Direction.Ascending, true);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test Image"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_ForSecondPage_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 1, 1, out totalRecords, "SortOrder", Direction.Ascending, true);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithSinglePage_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 2, out totalRecords, "SortOrder", Direction.Ascending, true);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.First().Name, Is.EqualTo("Test Image"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithDescendingOrder_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "SortOrder", Direction.Descending, true);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WitAlternateOrder_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending, true);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithFilterMatchingSome_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;

                var filter = QueryFactory.Create<IMedia>().Where(x => x.Name.Contains("File"));
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "SortOrder", Direction.Ascending, true,
                    filter);

                // Assert
                Assert.That(totalRecords, Is.EqualTo(1));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithFilterMatchingAll_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == 2);
                long totalRecords;

                var filter = QueryFactory.Create<IMedia>().Where(x => x.Name.Contains("Test"));
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "SortOrder", Direction.Ascending, true,
                    filter);

                // Assert
                Assert.That(totalRecords, Is.EqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test Image"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var medias = repository.GetAll(NodeDto.NodeIdSeed + 1, NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(medias, Is.Not.Null);
                Assert.That(medias.Any(), Is.True);
                Assert.That(medias.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var medias = repository.GetAll();

                // Assert
                Assert.That(medias, Is.Not.Null);
                Assert.That(medias.Any(), Is.True);
                Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                var exists = repository.Exists(NodeDto.NodeIdSeed + 1);
                var existsToo = repository.Exists(NodeDto.NodeIdSeed + 1);
                var doesntExists = repository.Exists(NodeDto.NodeIdSeed + 5);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(existsToo, Is.True);
                Assert.That(doesntExists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Count_On_MediaRepository()
        {
            // Arrange
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(unitOfWork, out mediaTypeRepository);

                // Act
                int level = 2;
                var query = QueryFactory.Create<IMedia>().Where(x => x.Level == level);
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            //Create and Save folder-Media -> (NodeDto.NodeIdSeed)
            var folderMediaType = ServiceContext.MediaTypeService.Get(1031);
            var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
            ServiceContext.MediaService.Save(folder, 0);

            //Create and Save image-Media -> (NodeDto.NodeIdSeed + 1)
            var imageMediaType = ServiceContext.MediaTypeService.Get(1032);
            var image = MockedMedia.CreateMediaImage(imageMediaType, folder.Id);
            ServiceContext.MediaService.Save(image, 0);

            //Create and Save file-Media -> (NodeDto.NodeIdSeed + 2)
            var fileMediaType = ServiceContext.MediaTypeService.Get(1033);
            var file = MockedMedia.CreateMediaFile(fileMediaType, folder.Id);
            ServiceContext.MediaService.Save(file, 0);
        }
    }
}