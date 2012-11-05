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
    public class ContentTypeRepositoryTest : BaseDatabaseFactoryTest
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
            var repository = new ContentTypeRepository(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var contentType = MockedContentTypes.CreateSimpleContentType();
            repository.AddOrUpdate(contentType);
            unitOfWork.Commit();

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
            Assert.That(contentType.Path.Contains(","), Is.True);
            Assert.That(contentType.SortOrder, Is.GreaterThan(0));
        }

        [Test]
        public void Can_Perform_Update_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var unitOfWork2 = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var repository2 = new ContentTypeRepository(unitOfWork2, InMemoryCacheProvider.Current);

            // Act
            var contentType = repository.Get(1046);

            contentType.Thumbnail = "Doc2.png";
            contentType.PropertyGroups["Content"].PropertyTypes.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
            {
                Alias = "subtitle",
                Name = "Subtitle",
                Description = "Optional Subtitle",
                HelpText = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            });
            repository2.AddOrUpdate(contentType);
            unitOfWork2.Commit();

            var dirty = ((ContentType) contentType).IsDirty();

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(dirty, Is.False);
            Assert.That(contentType.Thumbnail, Is.EqualTo("Doc2.png"));
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
        }

        [Test]
        public void Can_Perform_Delete_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var unitOfWork2 = provider.GetUnitOfWork();
            var unitOfWork3 = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var repository2 = new ContentTypeRepository(unitOfWork2, InMemoryCacheProvider.Current);
            var repository3 = new ContentTypeRepository(unitOfWork3, InMemoryCacheProvider.Current);

            // Act
            var contentType = MockedContentTypes.CreateSimpleContentType();
            repository.AddOrUpdate(contentType);
            unitOfWork.Commit();

            var contentType2 = repository2.Get(contentType.Id);
            repository2.Delete(contentType2);
            unitOfWork2.Commit();

            var exists = repository3.Exists(contentType.Id);

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public void Can_Perform_Get_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var contentType = repository.Get(1046);

            // Assert
            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.Id, Is.EqualTo(1046));
        }

        [Test]
        public void Can_Perform_GetAll_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            InMemoryCacheProvider.Current.Clear();

            // Act
            var contentTypes = repository.GetAll();
            int count =
                DatabaseContext.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                    new { NodeObjectType = new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB") });

            // Assert
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(count));
        }

        [Test]
        public void Can_Perform_Exists_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var exists = repository.Exists(1045);

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public void Can_Update_ContentType_With_PropertyType_Removed()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var unitOfWork2 = provider.GetUnitOfWork();
            var unitOfWork3 = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);
            var repository2 = new ContentTypeRepository(unitOfWork2, InMemoryCacheProvider.Current);
            var repository3 = new ContentTypeRepository(unitOfWork3, InMemoryCacheProvider.Current);
            var contentType = repository.Get(1046);

            // Act
            var contentType2 = repository2.Get(1046);
            contentType2.PropertyGroups["Meta"].PropertyTypes.Remove("metaDescription");
            repository2.AddOrUpdate(contentType2);
            unitOfWork2.Commit();

            var contentType3 = repository3.Get(1046);

            // Assert
            Assert.That(contentType3.PropertyTypes.Any(x => x.Alias == "metaDescription"), Is.False);
            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(contentType3.PropertyGroups.Count));
            Assert.That(contentType.PropertyTypes.Count(), Is.GreaterThan(contentType3.PropertyTypes.Count()));
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_SimpleTextpage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var contentType = repository.Get(1045);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_Textpage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new ContentTypeRepository(unitOfWork, InMemoryCacheProvider.Current);

            // Act
            var contentType = repository.Get(1046);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(2));
        }

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> 1045
            ContentType simpleContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.ContentTypeService.Save(simpleContentType);

            //Create and Save ContentType "textPage" -> 1046
            ContentType textpageContentType = MockedContentTypes.CreateTextpageContentType();
            ServiceContext.ContentTypeService.Save(textpageContentType);
        }
    }
}