using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Testing;
using Umbraco.Core.Services;

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

        private MediaRepository CreateRepository(IScopeProvider provider, out MediaTypeRepository mediaTypeRepository, AppCaches appCaches = null)
        {
            appCaches = appCaches ?? AppCaches;
            var scopeAccessor = (IScopeAccessor) provider;

            mediaTypeRepository = new MediaTypeRepository(scopeAccessor, appCaches, Logger);
            var tagRepository = new TagRepository(scopeAccessor, appCaches, Logger);
            var repository = new MediaRepository(scopeAccessor, appCaches, Logger, mediaTypeRepository, tagRepository, Mock.Of<IContentSection>(), Mock.Of<ILanguageRepository>());
            return repository;
        }

        [Test]
        public void CacheActiveForIntsAndGuids()
        {
            MediaTypeRepository mediaTypeRepository;

            var realCache = new AppCaches(
                new ObjectCacheAppCache(),
                new DictionaryCacheProvider(),
                new DictionaryCacheProvider(),
                new IsolatedCaches(t => new ObjectCacheAppCache()));

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out mediaTypeRepository, appCaches: realCache);

                var udb = (UmbracoDatabase)scope.Database;

                udb.EnableSqlCount = false;

                var mediaType = MockedContentTypes.CreateSimpleMediaType("umbTextpage1", "Textpage");
                var media = MockedMedia.CreateSimpleMedia(mediaType, "hello", -1);
                mediaTypeRepository.Save(mediaType);
                repository.Save(media);

                udb.EnableSqlCount = true;

                //go get it, this should already be cached since the default repository key is the INT
                var found = repository.Get(media.Id);
                Assert.AreEqual(0, udb.SqlCount);
                //retrieve again, this should use cache
                found = repository.Get(media.Id);
                Assert.AreEqual(0, udb.SqlCount);

                //reset counter
                udb.EnableSqlCount = false;
                udb.EnableSqlCount = true;

                //now get by GUID, this won't be cached yet because the default repo key is not a GUID
                found = repository.Get(media.Key);
                var sqlCount = udb.SqlCount;
                Assert.Greater(sqlCount, 0);
                //retrieve again, this should use cache now
                found = repository.Get(media.Key);
                Assert.AreEqual(sqlCount, udb.SqlCount);
            }
        }

        [Test]
        public void SaveMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                var mediaType = mediaTypeRepository.Get(1032);
                var image = MockedMedia.CreateMediaImage(mediaType, -1);

                // Act
                mediaTypeRepository.Save(mediaType);
                repository.Save(image);

                var fetched = repository.Get(image.Id);

                // Assert
                Assert.That(mediaType.HasIdentity, Is.True);
                Assert.That(image.HasIdentity, Is.True);

                TestHelper.AssertPropertyValuesAreEqual(image, fetched, "yyyy-MM-dd HH:mm:ss");
            }
        }

        [Test]
        public void SaveMediaMultiple()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                var mediaType = mediaTypeRepository.Get(1032);
                var file = MockedMedia.CreateMediaFile(mediaType, -1);

                // Act
                repository.Save(file);

                var image = MockedMedia.CreateMediaImage(mediaType, -1);
                repository.Save(image);

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
        public void GetMediaIsNotDirty()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var media = repository.Get(NodeDto.NodeIdSeed + 1);
                bool dirty = ((ICanBeDirty)media).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void UpdateMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Name = "Test File Updated";
                repository.Save(content);

                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
                Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
            }
        }

        [Test]
        public void DeleteMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var media = repository.Get(NodeDto.NodeIdSeed + 2);
                repository.Delete(media);

                var deleted = repository.Get(NodeDto.NodeIdSeed + 2);
                var exists = repository.Exists(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(deleted, Is.Null);
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void GetMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var media = repository.Get(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(media.Id, Is.EqualTo(NodeDto.NodeIdSeed + 1));
                Assert.That(media.CreateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(media.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(media.ParentId, Is.Not.EqualTo(0));
                Assert.That(media.Name, Is.EqualTo("Test Image"));
                Assert.That(media.SortOrder, Is.EqualTo(0));
                Assert.That(media.VersionId, Is.Not.EqualTo(0));
                Assert.That(media.ContentTypeId, Is.EqualTo(1032));
                Assert.That(media.Path, Is.Not.Empty);
                Assert.That(media.Properties.Any(), Is.True);
            }
        }

        [Test]
        public void QueryMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);
                var result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2)); //There should be two entities on level 2: File and Media
            }
        }

        [Test]
        public void QueryMedia_ContentTypeIdFilter()
        {
            // Arrange
            var folderMediaType = ServiceContext.MediaTypeService.Get(1031);
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                for (int i = 0; i < 10; i++)
                {
                    var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
                    repository.Save(folder);
                }
                

                var types = new[] { 1031 };
                var query = scope.SqlContext.Query<IMedia>().Where(x => types.Contains(x.ContentTypeId));
                var result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(11));
            }
        }

        [Ignore("Unsupported feature.")]
        [Test]
        public void QueryMedia_ContentTypeAliasFilter()
        {
            // we could support this, but it would require an extra join on the query,
            // and we don't absolutely need it now, so leaving it out for now

            // Arrange
            var folderMediaType = ServiceContext.MediaTypeService.Get(1031);
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                for (int i = 0; i < 10; i++)
                {
                    var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
                    repository.Save(folder);
                }
                

                var types = new[] { "Folder" };
                var query = scope.SqlContext.Query<IMedia>().Where(x => types.Contains(x.ContentType.Alias));
                var result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(11));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_FirstPage()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPage(query, 0, 1, out totalRecords, null, Ordering.By("SortOrder"));

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test Image"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_SecondPage()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPage(query, 1, 1, out totalRecords, null, Ordering.By("SortOrder"));

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_SinglePage()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPage(query, 0, 2, out totalRecords, null, Ordering.By("SortOrder"));

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.First().Name, Is.EqualTo("Test Image"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_DescendingOrder()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPage(query, 0, 1, out totalRecords, null, Ordering.By("SortOrder", Direction.Descending));

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_AlternateOrder()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);
                var result = repository.GetPage(query, 0, 1, out var totalRecords, null, Ordering.By("Name"));

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_FilterMatchingSome()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);

                var filter = scope.SqlContext.Query<IMedia>().Where(x => x.Name.Contains("File"));
                var result = repository.GetPage(query, 0, 1, out var totalRecords, filter, Ordering.By("SortOrder"));

                // Assert
                Assert.That(totalRecords, Is.EqualTo(1));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test File"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_FilterMatchingAll()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out _);

                // Act
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == 2);

                var filter = scope.SqlContext.Query<IMedia>().Where(x => x.Name.Contains("Test"));
                var result = repository.GetPage(query, 0, 1, out var totalRecords, filter, Ordering.By("SortOrder"));

                // Assert
                Assert.That(totalRecords, Is.EqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Test Image"));
            }
        }

        [Test]
        public void GetAllMediaByIds()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var medias = repository.GetMany(NodeDto.NodeIdSeed + 1, NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(medias, Is.Not.Null);
                Assert.That(medias.Any(), Is.True);
                Assert.That(medias.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void GetAllMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                var medias = repository.GetMany();

                // Assert
                Assert.That(medias, Is.Not.Null);
                Assert.That(medias.Any(), Is.True);
                Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));

                medias = repository.GetMany(medias.Select(x => x.Id).ToArray());
                Assert.That(medias, Is.Not.Null);
                Assert.That(medias.Any(), Is.True);
                Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));

                medias = ((IReadRepository<Guid, IMedia>)repository).GetMany(medias.Select(x => x.Key).ToArray());
                Assert.That(medias, Is.Not.Null);
                Assert.That(medias.Any(), Is.True);
                Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));
            }
        }

        [Test]
        public void ExistMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

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
        public void CountMedia()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MediaTypeRepository mediaTypeRepository;
                var repository = CreateRepository(provider, out mediaTypeRepository);

                // Act
                int level = 2;
                var query = scope.SqlContext.Query<IMedia>().Where(x => x.Level == level);
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
