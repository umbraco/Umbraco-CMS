// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class MediaRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUpTestData() => CreateTestData();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private ITemplateRepository TemplateRepository => GetRequiredService<ITemplateRepository>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    // Makes handing IDs easier, these are set by CreateTestData
    private Media _testFolder;
    private Media _testImage;
    private Media _testFile;

    private MediaRepository CreateRepository(IScopeProvider provider, out MediaTypeRepository mediaTypeRepository, AppCaches appCaches = null)
    {
        appCaches ??= AppCaches.NoCache;
        var scopeAccessor = (IScopeAccessor)provider;
        var globalSettings = new GlobalSettings();
        var commonRepository =
            new ContentTypeCommonRepository(scopeAccessor, TemplateRepository, appCaches, ShortStringHelper);
        var languageRepository =
            new LanguageRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<LanguageRepository>());
        mediaTypeRepository = new MediaTypeRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<MediaTypeRepository>(), commonRepository, languageRepository, ShortStringHelper);
        var tagRepository = new TagRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TagRepository>());
        var relationTypeRepository = new RelationTypeRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>());
        var entityRepository = new EntityRepository(scopeAccessor, AppCaches.Disabled);
        var relationRepository = new RelationRepository(scopeAccessor, LoggerFactory.CreateLogger<RelationRepository>(), relationTypeRepository, entityRepository);
        var propertyEditors =
            new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        var mediaUrlGenerators = new MediaUrlGeneratorCollection(() => Enumerable.Empty<IMediaUrlGenerator>());
        var dataValueReferences =
            new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());
        var repository = new MediaRepository(
            scopeAccessor,
            appCaches,
            LoggerFactory.CreateLogger<MediaRepository>(),
            LoggerFactory,
            mediaTypeRepository,
            tagRepository,
            Mock.Of<ILanguageRepository>(),
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            mediaUrlGenerators,
            dataValueReferences,
            DataTypeService,
            JsonSerializer,
            Mock.Of<IEventAggregator>());
        return repository;
    }

    [Test]
    public void CacheActiveForIntsAndGuids()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository, realCache);

            var udb = ScopeAccessor.AmbientScope.Database;

            udb.EnableSqlCount = false;

            var mediaType = MediaTypeBuilder.CreateSimpleMediaType("umbTextpage1", "Textpage");
            mediaTypeRepository.Save(mediaType);

            var media = MediaBuilder.CreateSimpleMedia(mediaType, "hello", -1);
            repository.Save(media);

            udb.EnableSqlCount = true;

            // go get it, this should already be cached since the default repository key is the INT
            var found = repository.Get(media.Id);
            Assert.AreEqual(0, udb.SqlCount);

            // retrieve again, this should use cache
            found = repository.Get(media.Id);
            Assert.AreEqual(0, udb.SqlCount);

            // reset counter
            udb.EnableSqlCount = false;
            udb.EnableSqlCount = true;

            // now get by GUID, this won't be cached yet because the default repo key is not a GUID
            found = repository.Get(media.Key);
            var sqlCount = udb.SqlCount;
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            var mediaType = mediaTypeRepository.Get(1032);
            var image = MediaBuilder.CreateMediaImage(mediaType, -1);

            // Act
            mediaTypeRepository.Save(mediaType);
            repository.Save(image);

            var fetched = repository.Get(image.Id);

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            var mediaType = mediaTypeRepository.Get(1032);
            var file = MediaBuilder.CreateMediaFile(mediaType, -1);

            // Act
            repository.Save(file);

            var image = MediaBuilder.CreateMediaImage(mediaType, -1);
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var media = repository.Get(_testImage.Id);
            var dirty = media.IsDirty();

            // Assert
            Assert.That(dirty, Is.False);
        }
    }

    [Test]
    public void UpdateMedia()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var content = repository.Get(_testFile.Id);
            content.Name = "Test File Updated";
            repository.Save(content);

            var updatedContent = repository.Get(_testFile.Id);

            // Assert
            Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
            Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
        }
    }

    [Test]
    public void DeleteMedia()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var media = repository.Get(_testFile.Id);
            repository.Delete(media);

            var deleted = repository.Get(_testFile.Id);
            var exists = repository.Exists(_testFile.Id);

            // Assert
            Assert.That(deleted, Is.Null);
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public void GetMedia()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var media = repository.Get(_testImage.Id);

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
            var result = repository.Get(query);

            // Assert
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2)); // There should be two entities on level 2: File and Media
        }
    }

    [Test]
    public void QueryMedia_ContentTypeIdFilter()
    {
        // Arrange
        var folderMediaType = MediaTypeService.Get(1031);
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            for (var i = 0; i < 10; i++)
            {
                var folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
                repository.Save(folder);
            }

            int[] types = { 1031 };
            var query = provider.CreateQuery<IMedia>().Where(x => types.Contains(x.ContentTypeId));
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
        var folderMediaType = MediaTypeService.Get(1031);
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            for (var i = 0; i < 10; i++)
            {
                var folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
                repository.Save(folder);
            }

            string[] types = { "Folder" };
            var query = provider.CreateQuery<IMedia>().Where(x => types.Contains(x.ContentType.Alias));
            var result = repository.Get(query);

            // Assert
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(11));
        }
    }

    [Test]
    public void GetPagedResultsByQuery_FirstPage()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 0, 1, out var totalRecords, null, Ordering.By("SortOrder")).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 1, 1, out var totalRecords, null, Ordering.By("SortOrder")).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 0, 2, out var totalRecords, null, Ordering.By("SortOrder")).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 0, 1, out var totalRecords, null, Ordering.By("SortOrder", Direction.Descending)).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 0, 1, out var totalRecords, null, Ordering.By("Name")).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);

            var filter = provider.CreateQuery<IMedia>().Where(x => x.Name.Contains("File"));
            var result = repository.GetPage(query, 0, 1, out var totalRecords, filter, Ordering.By("SortOrder")).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out _);

            // Act
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == 2);

            var filter = provider.CreateQuery<IMedia>().Where(x => x.Name.Contains("Test"));
            var result = repository.GetPage(query, 0, 1, out var totalRecords, filter, Ordering.By("SortOrder")).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var medias = repository.GetMany(_testImage.Id, _testFile.Id).ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var medias = repository.GetMany().ToArray();

            // Assert
            Assert.That(medias, Is.Not.Null);
            Assert.That(medias.Any(), Is.True);
            Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));

            medias = repository.GetMany(medias.Select(x => x.Id).ToArray()).ToArray();
            Assert.That(medias, Is.Not.Null);
            Assert.That(medias.Any(), Is.True);
            Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));

            medias = ((IReadRepository<Guid, IMedia>)repository).GetMany(medias.Select(x => x.Key).ToArray()).ToArray();
            Assert.That(medias, Is.Not.Null);
            Assert.That(medias.Any(), Is.True);
            Assert.That(medias.Count(), Is.GreaterThanOrEqualTo(3));
        }
    }

    [Test]
    public void ExistMedia()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var exists = repository.Exists(_testImage.Id);
            var existsToo = repository.Exists(_testImage.Id);
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider, out var mediaTypeRepository);

            // Act
            var level = 2;
            var query = provider.CreateQuery<IMedia>().Where(x => x.Level == level);
            var result = repository.Count(query);

            // Assert
            Assert.That(result, Is.GreaterThanOrEqualTo(2));
        }
    }

    public void CreateTestData()
    {
        // Create and Save folder-Media -> (1051)
        var folderMediaType = MediaTypeService.Get(1031);
        _testFolder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
        MediaService.Save(_testFolder, 0);

        // Create and Save image-Media -> (1052)
        var imageMediaType = MediaTypeService.Get(1032);
        _testImage = MediaBuilder.CreateMediaImage(imageMediaType, _testFolder.Id);
        MediaService.Save(_testImage, 0);

        // Create and Save file-Media -> (1053)
        var fileMediaType = MediaTypeService.Get(1033);
        _testFile = MediaBuilder.CreateMediaFile(fileMediaType, _testFolder.Id);
        MediaService.Save(_testFile, 0);
    }
}
