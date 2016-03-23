using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class MediaTypeRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        private MediaTypeRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            return new MediaTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);            
        }

        private EntityContainerRepository CreateContainerRepository(IDatabaseUnitOfWork unitOfWork)
        {
            return new EntityContainerRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Constants.ObjectTypes.MediaTypeContainerGuid);
        }

        [Test]
        public void Can_Move()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            using (var repository = CreateRepository(unitOfWork))
            {
                var container1 = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah1" };
                containerRepository.AddOrUpdate(container1);
                unitOfWork.Commit();

                var container2 = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah2", ParentId = container1.Id };
                containerRepository.AddOrUpdate(container2);
                unitOfWork.Commit();

                var contentType = (IMediaType)MockedContentTypes.CreateVideoMediaType();
                contentType.ParentId = container2.Id;
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                //create a 
                var contentType2 = (IMediaType)new MediaType(contentType, "hello")
                {
                    Name = "Blahasdfsadf"
                };
                contentType.ParentId = contentType.Id;
                repository.AddOrUpdate(contentType2);
                unitOfWork.Commit();

                var result = repository.Move(contentType, container1).ToArray();
                unitOfWork.Commit();

                Assert.AreEqual(2, result.Count());

                //re-get
                contentType = repository.Get(contentType.Id);
                contentType2 = repository.Get(contentType2.Id);

                Assert.AreEqual(container1.Id, contentType.ParentId);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType.Id).OriginalPath, contentType.Path);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType2.Id).OriginalPath, contentType2.Path);
            }
            
        }

        [Test]
        public void Can_Create_Container()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            EntityContainer container;
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            {
                container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();
                Assert.That(container.Id, Is.GreaterThan(0));
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            {
                var found = containerRepository.Get(container.Id);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Can_Delete_Container()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            EntityContainer container;
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            {
                container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();
                Assert.That(container.Id, Is.GreaterThan(0));
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            {
                // Act
                containerRepository.Delete(container);
                unitOfWork.Commit();
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            {
                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);
            }
        }

        [Test]
        public void Can_Create_Container_Containing_Media_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            using (var repository = CreateRepository(unitOfWork))
            {
                var container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();

                var contentType = MockedContentTypes.CreateVideoMediaType();
                contentType.ParentId = container.Id;
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                Assert.AreEqual(container.Id, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Delete_Container_Containing_Media_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            EntityContainer container;
            IMediaType contentType;
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            using (var repository = CreateRepository(unitOfWork))
            {
                container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();

                contentType = MockedContentTypes.CreateVideoMediaType();
                contentType.ParentId = container.Id;
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork))
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                containerRepository.Delete(container);
                unitOfWork.Commit();

                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);

                contentType = repository.Get(contentType.Id);
                Assert.IsNotNull(contentType);
                Assert.AreEqual(-1, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var contentType = MockedContentTypes.CreateVideoMediaType();
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));    
            }

            
        }

        [Test]
        public void Can_Perform_Update_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var videoMediaType = MockedContentTypes.CreateVideoMediaType();
                repository.AddOrUpdate(videoMediaType);
                unitOfWork.Commit();

                // Act
                var mediaType = repository.Get(NodeDto.NodeIdSeed);

                mediaType.Thumbnail = "Doc2.png";
                mediaType.PropertyGroups["Media"].PropertyTypes.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "subtitle")
                    {
                        Name = "Subtitle",
                        Description = "Optional Subtitle",
                        Mandatory = false,
                        SortOrder = 1,
                        DataTypeDefinitionId = -88
                    });
                repository.AddOrUpdate(mediaType);
                unitOfWork.Commit();

                var dirty = ((MediaType) mediaType).IsDirty();

                // Assert
                Assert.That(mediaType.HasIdentity, Is.True);
                Assert.That(dirty, Is.False);
                Assert.That(mediaType.Thumbnail, Is.EqualTo("Doc2.png"));
                Assert.That(mediaType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Delete_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var mediaType = MockedContentTypes.CreateVideoMediaType();
                repository.AddOrUpdate(mediaType);
                unitOfWork.Commit();

                var contentType2 = repository.Get(mediaType.Id);
                repository.Delete(contentType2);
                unitOfWork.Commit();

                var exists = repository.Exists(mediaType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var mediaType = repository.Get(1033); //File

                // Assert
                Assert.That(mediaType, Is.Not.Null);
                Assert.That(mediaType.Id, Is.EqualTo(1033));
                Assert.That(mediaType.Name, Is.EqualTo(Constants.Conventions.MediaTypes.File));
            }
        }

        [Test]
        public void Can_Perform_Get_By_Guid_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var mediaType = repository.Get(1033); //File

                // Act
                mediaType = repository.Get(mediaType.Key); 

                // Assert
                Assert.That(mediaType, Is.Not.Null);
                Assert.That(mediaType.Id, Is.EqualTo(1033));
                Assert.That(mediaType.Name, Is.EqualTo(Constants.Conventions.MediaTypes.File));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var mediaTypes = repository.GetAll();
                int count =
                    DatabaseContext.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new {NodeObjectType = new Guid(Constants.ObjectTypes.MediaType)});

                // Assert
                Assert.That(mediaTypes.Any(), Is.True);
                Assert.That(mediaTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Guid_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var allGuidIds = repository.GetAll().Select(x => x.Key).ToArray();

                // Act

                var mediaTypes = ((IReadRepository<Guid, IMediaType>)repository).GetAll(allGuidIds);

                int count =
                    DatabaseContext.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = new Guid(Constants.ObjectTypes.MediaType) });

                // Assert
                Assert.That(mediaTypes.Any(), Is.True);
                Assert.That(mediaTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var exists = repository.Exists(1032); //Image

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Update_MediaType_With_PropertyType_Removed()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var mediaType = MockedContentTypes.CreateVideoMediaType();
                repository.AddOrUpdate(mediaType);
                unitOfWork.Commit();

                // Act
                var mediaTypeV2 = repository.Get(NodeDto.NodeIdSeed);
                mediaTypeV2.PropertyGroups["Media"].PropertyTypes.Remove("title");
                repository.AddOrUpdate(mediaTypeV2);
                unitOfWork.Commit();

                var mediaTypeV3 = repository.Get(NodeDto.NodeIdSeed);

                // Assert
                Assert.That(mediaTypeV3.PropertyTypes.Any(x => x.Alias == "title"), Is.False);
                Assert.That(mediaTypeV2.PropertyGroups.Count, Is.EqualTo(mediaTypeV3.PropertyGroups.Count));
                Assert.That(mediaTypeV2.PropertyTypes.Count(), Is.EqualTo(mediaTypeV3.PropertyTypes.Count()));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_Video_MediaType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var mediaType = MockedContentTypes.CreateVideoMediaType();
                repository.AddOrUpdate(mediaType);
                unitOfWork.Commit();

                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_File_MediaType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var contentType = repository.Get(1033); //File

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}