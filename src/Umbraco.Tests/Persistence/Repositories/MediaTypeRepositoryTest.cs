using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class MediaTypeRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = new MediaTypeRepository(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

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

        [Test]
        public void Can_Perform_Update_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var unitOfWork2 = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var repository2 = new MediaTypeRepository(unitOfWork2, InMemoryCacheProvider.Current);
            var videoMediaType = MockedContentTypes.CreateVideoMediaType();
            repository.AddOrUpdate(videoMediaType);
            unitOfWork.Commit();

            // Act
            var mediaType = repository.Get(1045);

            mediaType.Thumbnail = "Doc2.png";
            mediaType.PropertyGroups["Media"].PropertyTypes.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
            {
                Alias = "subtitle",
                Name = "Subtitle",
                Description = "Optional Subtitle",
                HelpText = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            });
            repository2.AddOrUpdate(mediaType);
            unitOfWork2.Commit();

            var dirty = ((MediaType)mediaType).IsDirty();

            // Assert
            Assert.That(mediaType.HasIdentity, Is.True);
            Assert.That(dirty, Is.False);
            Assert.That(mediaType.Thumbnail, Is.EqualTo("Doc2.png"));
            Assert.That(mediaType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
        }

        [Test]
        public void Can_Perform_Delete_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var unitOfWork2 = provider.GetUnitOfWork();
            var unitOfWork3 = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var repository2 = new MediaTypeRepository(unitOfWork2, InMemoryCacheProvider.Current);
            var repository3 = new MediaTypeRepository(unitOfWork3, InMemoryCacheProvider.Current);

            // Act
            var mediaType = MockedContentTypes.CreateVideoMediaType();
            repository.AddOrUpdate(mediaType);
            unitOfWork.Commit();

            var contentType2 = repository2.Get(mediaType.Id);
            repository2.Delete(contentType2);
            unitOfWork2.Commit();

            var exists = repository3.Exists(mediaType.Id);

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public void Can_Perform_Get_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var mediaType = repository.Get(1033);//File

            // Assert
            Assert.That(mediaType, Is.Not.Null);
            Assert.That(mediaType.Id, Is.EqualTo(1033));
            Assert.That(mediaType.Name, Is.EqualTo("File"));
        }

        [Test]
        public void Can_Perform_GetAll_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            InMemoryCacheProvider.Current.Clear();

            // Act
            var mediaTypes = repository.GetAll();
            int count =
                DatabaseContext.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                    new { NodeObjectType = new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e") });

            // Assert
            Assert.That(mediaTypes.Any(), Is.True);
            Assert.That(mediaTypes.Count(), Is.EqualTo(count));
        }

        [Test]
        public void Can_Perform_Exists_On_MediaTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var exists = repository.Exists(1032);//Image

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public void Can_Update_MediaType_With_PropertyType_Removed()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var unitOfWork2 = provider.GetUnitOfWork();
            var unitOfWork3 = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var repository2 = new MediaTypeRepository(unitOfWork2, InMemoryCacheProvider.Current);
            var repository3 = new MediaTypeRepository(unitOfWork3, InMemoryCacheProvider.Current);
            var mediaType = MockedContentTypes.CreateVideoMediaType();
            repository.AddOrUpdate(mediaType);
            unitOfWork.Commit();

            // Act
            var mediaTypeV2 = repository2.Get(1045);
            mediaTypeV2.PropertyGroups["Media"].PropertyTypes.Remove("title");
            repository2.AddOrUpdate(mediaTypeV2);
            unitOfWork2.Commit();

            var mediaTypeV3 = repository3.Get(1045);

            // Assert
            Assert.That(mediaTypeV3.PropertyTypes.Any(x => x.Alias == "title"), Is.False);
            Assert.That(mediaType.PropertyGroups.Count, Is.EqualTo(mediaTypeV3.PropertyGroups.Count));
            Assert.That(mediaType.PropertyTypes.Count(), Is.GreaterThan(mediaTypeV3.PropertyTypes.Count()));
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_Video_MediaType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var mediaType = MockedContentTypes.CreateVideoMediaType();
            repository.AddOrUpdate(mediaType);
            unitOfWork.Commit();

            // Act
            var contentType = repository.Get(1045);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_File_MediaType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new MediaTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var contentType = repository.Get(1033);//File

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}