// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MediaRepositoryTest : UmbracoIntegrationTest
    {
        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

        private ITemplateRepository TemplateRepository => GetRequiredService<ITemplateRepository>();

        private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

        private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

        // Makes handing IDs easier, these are set by CreateTestData
        private Media _testFolder;
        private Media _testImage;
        private Media _testFile;

        [SetUp]
        public void SetUpTestData() => CreateTestData();

        private MediaRepository CreateRepository(IScopeProvider provider, out MediaTypeRepository mediaTypeRepository, AppCaches appCaches = null)
        {
            appCaches ??= AppCaches.NoCache;
            var scopeAccessor = (IScopeAccessor)provider;
            var globalSettings = new GlobalSettings();
            var commonRepository = new ContentTypeCommonRepository(scopeAccessor, TemplateRepository, appCaches, ShortStringHelper);
            var languageRepository = new LanguageRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<LanguageRepository>());
            mediaTypeRepository = new MediaTypeRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<MediaTypeRepository>(), commonRepository, languageRepository, ShortStringHelper);
            var tagRepository = new TagRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TagRepository>());
            var relationTypeRepository = new RelationTypeRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>());
            var entityRepository = new EntityRepository(scopeAccessor, AppCaches.Disabled);
            var relationRepository = new RelationRepository(scopeAccessor, LoggerFactory.CreateLogger<RelationRepository>(), relationTypeRepository, entityRepository);
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
            var mediaUrlGenerators = new MediaUrlGeneratorCollection(() => Enumerable.Empty<IMediaUrlGenerator>());
            var dataValueReferences = new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new MediaRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<MediaRepository>(), LoggerFactory, mediaTypeRepository, tagRepository, Mock.Of<ILanguageRepository>(), relationRepository, relationTypeRepository, propertyEditors, mediaUrlGenerators, dataValueReferences, DataTypeService, JsonSerializer, Mock.Of<IEventAggregator>());
            return repository;
        }

        [Test]
        public void CacheActiveForIntsAndGuids()
        {
            var realCache = new AppCaches(
                new ObjectCacheAppCache(),
                new DictionaryAppCache(),
                new IsolatedCaches(t => new ObjectCacheAppCache()));

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository, appCaches: realCache);

                IUmbracoDatabase udb = ScopeAccessor.AmbientScope.Database;

                udb.EnableSqlCount = false;

                MediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("umbTextpage1", "Textpage");
                mediaTypeRepository.Save(mediaType);

                Media media = MediaBuilder.CreateSimpleMedia(mediaType, "hello", -1);
                repository.Save(media);

                udb.EnableSqlCount = true;

                // go get it, this should already be cached since the default repository key is the INT
                IMedia found = repository.Get(media.Id);
                Assert.AreEqual(0, udb.SqlCount);

                // retrieve again, this should use cache
                found = repository.Get(media.Id);
                Assert.AreEqual(0, udb.SqlCount);

                // reset counter
                udb.EnableSqlCount = false;
                udb.EnableSqlCount = true;

                // now get by GUID, this won't be cached yet because the default repo key is not a GUID
                found = repository.Get(media.Key);
                int sqlCount = udb.SqlCount;
                Assert.Greater(sqlCount, 0);

                // retrieve again, this should use cache now
                found = repository.Get(media.Key);
                Assert.AreEqual(sqlCount, udb.SqlCount);
            }
        }

        [Test]
        public void SaveMedia()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                IMediaType mediaType = mediaTypeRepository.Get(1032);
                Media image = MediaBuilder.CreateMediaImage(mediaType, -1);

                // Act
                mediaTypeRepository.Save(mediaType);
                repository.Save(image);

                IMedia fetched = repository.Get(image.Id);

                // Assert
                Assert.That(mediaType.HasIdentity, Is.True);
                Assert.That(image.HasIdentity, Is.True);

                TestHelper.AssertPropertyValuesAreEqual(image, fetched);
            }
        }

        [Test]
        public void SaveMediaMultiple()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                IMediaType mediaType = mediaTypeRepository.Get(1032);
                Media file = MediaBuilder.CreateMediaFile(mediaType, -1);

                // Act
                repository.Save(file);

                Media image = MediaBuilder.CreateMediaImage(mediaType, -1);
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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IMedia media = repository.Get(_testImage.Id);
                bool dirty = ((ICanBeDirty)media).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void UpdateMedia()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IMedia content = repository.Get(_testFile.Id);
                content.Name = "Test File Updated";
                repository.Save(content);

                IMedia updatedContent = repository.Get(_testFile.Id);

                // Assert
                Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
                Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
            }
        }

        [Test]
        public void DeleteMedia()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IMedia media = repository.Get(_testFile.Id);
                repository.Delete(media);

                IMedia deleted = repository.Get(_testFile.Id);
                bool exists = repository.Exists(_testFile.Id);

                // Assert
                Assert.That(deleted, Is.Null);
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void GetMedia()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IMedia media = repository.Get(_testImage.Id);

                // Assert
                Assert.That(media.Id, Is.EqualTo(_testImage.Id));
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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
                IEnumerable<IMedia> result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2)); // There should be two entities on level 2: File and Media
            }
        }

        [Test]
        public void QueryMedia_ContentTypeIdFilter()
        {
            // Arrange
            IMediaType folderMediaType = MediaTypeService.Get(1031);
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                for (int i = 0; i < 10; i++)
                {
                    Media folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
                    repository.Save(folder);
                }

                int[] types = new[] { 1031 };
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => types.Contains(x.ContentTypeId));
                IEnumerable<IMedia> result = repository.Get(query);

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
            IMediaType folderMediaType = MediaTypeService.Get(1031);
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                for (int i = 0; i < 10; i++)
                {
                    Media folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
                    repository.Save(folder);
                }

                string[] types = new[] { "Folder" };
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => types.Contains(x.ContentType.Alias));
                IEnumerable<IMedia> result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(11));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_FirstPage()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
                IEnumerable<IMedia> result = repository.GetPage(query, 0, 1, out long totalRecords, null, Ordering.By("SortOrder"));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
                IEnumerable<IMedia> result = repository.GetPage(query, 1, 1, out long totalRecords, null, Ordering.By("SortOrder"));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
                IEnumerable<IMedia> result = repository.GetPage(query, 0, 2, out long totalRecords, null, Ordering.By("SortOrder"));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
                IEnumerable<IMedia> result = repository.GetPage(query, 0, 1, out long totalRecords, null, Ordering.By("SortOrder", Direction.Descending));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
                IEnumerable<IMedia> result = repository.GetPage(query, 0, 1, out long totalRecords, null, Ordering.By("Name"));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);

                IQuery<IMedia> filter = provider.CreateQuery<IMedia>().Where(x => x.Name.Contains("File"));
                IEnumerable<IMedia> result = repository.GetPage(query, 0, 1, out long totalRecords, filter, Ordering.By("SortOrder"));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out _);

                // Act
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);

                IQuery<IMedia> filter = provider.CreateQuery<IMedia>().Where(x => x.Name.Contains("Test"));
                IEnumerable<IMedia> result = repository.GetPage(query, 0, 1, out long totalRecords, filter, Ordering.By("SortOrder"));

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IEnumerable<IMedia> medias = repository.GetMany(_testImage.Id, _testFile.Id);

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                IEnumerable<IMedia> medias = repository.GetMany();

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                bool exists = repository.Exists(_testImage.Id);
                bool existsToo = repository.Exists(_testImage.Id);
                bool doesntExists = repository.Exists(NodeDto.NodeIdSeed + 5);

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaRepository repository = CreateRepository(provider, out MediaTypeRepository mediaTypeRepository);

                // Act
                int level = 2;
                IQuery<IMedia> query = provider.CreateQuery<IMedia>().Where(x => x.Level == level);
                int result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        public void CreateTestData()
        {
            // Create and Save folder-Media -> (1051)
            IMediaType folderMediaType = MediaTypeService.Get(1031);
            _testFolder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
            MediaService.Save(_testFolder, 0);

            // Create and Save image-Media -> (1052)
            IMediaType imageMediaType = MediaTypeService.Get(1032);
            _testImage = MediaBuilder.CreateMediaImage(imageMediaType, _testFolder.Id);
            MediaService.Save(_testImage, 0);

            // Create and Save file-Media -> (1053)
            IMediaType fileMediaType = MediaTypeService.Get(1033);
            _testFile = MediaBuilder.CreateMediaFile(fileMediaType, _testFolder.Id);
            MediaService.Save(_testFile, 0);
        }
    }
}
