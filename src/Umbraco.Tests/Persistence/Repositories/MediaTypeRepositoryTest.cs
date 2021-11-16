using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MediaTypeRepositoryTest : TestWithDatabaseBase
    {
        private MediaTypeRepository CreateRepository(IScopeProvider provider)
        {
            var cacheHelper = AppCaches.Disabled;
            var templateRepository = new TemplateRepository((IScopeAccessor)provider, cacheHelper, Logger, TestObjects.GetFileSystemsMock());
            var commonRepository = new ContentTypeCommonRepository((IScopeAccessor)provider, templateRepository, AppCaches);
            var languageRepository = new LanguageRepository((IScopeAccessor)provider, AppCaches, Logger);
            return new MediaTypeRepository((IScopeAccessor) provider, AppCaches.Disabled, Logger, commonRepository, languageRepository);
        }

        private EntityContainerRepository CreateContainerRepository(IScopeProvider provider)
        {
            return new EntityContainerRepository((IScopeAccessor) provider, AppCaches.Disabled, Logger, Constants.ObjectTypes.MediaTypeContainer);

        }

        [Test]
        public void Can_Move()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(provider);
                var repository = CreateRepository(provider);

                var container1 = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah1" };
                containerRepository.Save(container1);
                

                var container2 = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah2", ParentId = container1.Id };
                containerRepository.Save(container2);
                

                var contentType = (IMediaType)MockedContentTypes.CreateNewMediaType();
                contentType.ParentId = container2.Id;
                repository.Save(contentType);
                

                //create a
                var contentType2 = (IMediaType)new MediaType(contentType, "hello")
                {
                    Name = "Blahasdfsadf"
                };
                contentType.ParentId = contentType.Id;
                repository.Save(contentType2);
                

                var result = repository.Move(contentType, container1).ToArray();
                

                Assert.AreEqual(2, result.Length);

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);
                
                Assert.That(container.Id, Is.GreaterThan(0));

                var found = containerRepository.Get(container.Id);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Can_Delete_Container()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);
                
                Assert.That(container.Id, Is.GreaterThan(0));

                // Act
                containerRepository.Delete(container);
                

                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);
            }
        }

        [Test]
        public void Can_Create_Container_Containing_Media_Types()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(provider);
                var repository = CreateRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);
                

                var contentType = MockedContentTypes.CreateNewMediaType();
                contentType.ParentId = container.Id;
                repository.Save(contentType);
                

                Assert.AreEqual(container.Id, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Delete_Container_Containing_Media_Types()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(provider);
                var repository = CreateRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);
                

                IMediaType contentType = MockedContentTypes.CreateNewMediaType();
                contentType.ParentId = container.Id;
                repository.Save(contentType);
                

                // Act
                containerRepository.Delete(container);
                

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var contentType = MockedContentTypes.CreateNewMediaType();
                repository.Save(contentType);
                

                var fetched = repository.Get(contentType.Id);

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));

                TestHelper.AssertPropertyValuesAreEqual(contentType, fetched, "yyyy-MM-dd HH:mm:ss", ignoreProperties: new[] { "UpdateDate" });
            }


        }

        [Test]
        public void Can_Perform_Update_On_MediaTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var videoMediaType = MockedContentTypes.CreateNewMediaType();
                repository.Save(videoMediaType);
                

                // Act
                var mediaType = repository.Get(NodeDto.NodeIdSeed);

                mediaType.Thumbnail = "Doc2.png";
                mediaType.PropertyGroups["Media"].PropertyTypes.Add(new PropertyType("test", ValueStorageType.Ntext, "subtitle")
                    {
                        Name = "Subtitle",
                        Description = "Optional Subtitle",
                        Mandatory = false,
                        SortOrder = 1,
                        DataTypeId = -88
                    });
                repository.Save(mediaType);
                

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var mediaType = MockedContentTypes.CreateNewMediaType();
                repository.Save(mediaType);
                

                var contentType2 = repository.Get(mediaType.Id);
                repository.Delete(contentType2);
                

                var exists = repository.Exists(mediaType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_MediaTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var mediaTypes = repository.GetMany();
                int count =
                    scope.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = Constants.ObjectTypes.MediaType });

                // Assert
                Assert.That(mediaTypes.Any(), Is.True);
                Assert.That(mediaTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Guid_On_MediaTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var allGuidIds = repository.GetMany().Select(x => x.Key).ToArray();

                // Act

                var mediaTypes = ((IReadRepository<Guid, IMediaType>)repository).GetMany(allGuidIds);

                int count =
                    scope.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = Constants.ObjectTypes.MediaType });

                // Assert
                Assert.That(mediaTypes.Any(), Is.True);
                Assert.That(mediaTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_MediaTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var mediaType = MockedContentTypes.CreateNewMediaType();
                repository.Save(mediaType);
                

                // Act
                var mediaTypeV2 = repository.Get(NodeDto.NodeIdSeed);
                mediaTypeV2.PropertyGroups["Media"].PropertyTypes.Remove("title");
                repository.Save(mediaTypeV2);
                

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var mediaType = MockedContentTypes.CreateNewMediaType();
                repository.Save(mediaType);
                

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

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
