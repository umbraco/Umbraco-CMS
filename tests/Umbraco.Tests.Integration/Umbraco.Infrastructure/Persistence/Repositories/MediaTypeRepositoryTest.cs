// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaTypeRepositoryTest : UmbracoIntegrationTest
{
    private IContentTypeCommonRepository CommonRepository => GetRequiredService<IContentTypeCommonRepository>();

    private ILanguageRepository LanguageRepository => GetRequiredService<ILanguageRepository>();

    [Test]
    public void Can_Move()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var containerRepository = CreateContainerRepository(provider);
            var repository = CreateRepository(provider);

            var container1 = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah1" };
            containerRepository.Save(container1);

            var container2 =
                new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah2", ParentId = container1.Id };
            containerRepository.Save(container2);

            IMediaType contentType =
                MediaTypeBuilder.CreateNewMediaType();
            contentType.ParentId = container2.Id;
            repository.Save(contentType);

            // create a
            var contentType2 =
                (IMediaType)new MediaType(ShortStringHelper, contentType, "hello") { Name = "Blahasdfsadf" };
            contentType.ParentId = contentType.Id;
            repository.Save(contentType2);

            var result = repository.Move(contentType, container1).ToArray();

            Assert.That(result, Has.Length.EqualTo(2));

            // re-get
            contentType = repository.Get(contentType.Id);
            contentType2 = repository.Get(contentType2.Id);

            Assert.That(contentType.ParentId, Is.EqualTo(container1.Id));
            Assert.That(contentType.Path, Is.Not.EqualTo(result.Single(x => x.Entity.Id == contentType.Id).OriginalPath));
            Assert.That(contentType2.Path, Is.Not.EqualTo(result.Single(x => x.Entity.Id == contentType2.Id).OriginalPath));
        }
    }

    [Test]
    public void Can_Create_Container()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var containerRepository = CreateContainerRepository(provider);

            var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
            containerRepository.Save(container);

            Assert.That(container.Id, Is.GreaterThan(0));

            var found = containerRepository.Get(container.Id);
            Assert.That(found, Is.Not.Null);
        }
    }

    [Test]
    public void Can_Delete_Container()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var containerRepository = CreateContainerRepository(provider);

            var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
            containerRepository.Save(container);

            Assert.That(container.Id, Is.GreaterThan(0));

            // Act
            containerRepository.Delete(container);

            var found = containerRepository.Get(container.Id);
            Assert.That(found, Is.Null);
        }
    }

    [Test]
    public void Can_Create_Container_Containing_Media_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var containerRepository = CreateContainerRepository(provider);
            var repository = CreateRepository(provider);

            var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
            containerRepository.Save(container);

            var contentType =
                MediaTypeBuilder.CreateSimpleMediaType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup");
            contentType.ParentId = container.Id;
            repository.Save(contentType);

            Assert.That(contentType.ParentId, Is.EqualTo(container.Id));
        }
    }

    [Test]
    public void Can_Delete_Container_Containing_Media_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var containerRepository = CreateContainerRepository(provider);
            var repository = CreateRepository(provider);

            var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
            containerRepository.Save(container);

            IMediaType contentType =
                MediaTypeBuilder.CreateSimpleMediaType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup");
            contentType.ParentId = container.Id;
            repository.Save(contentType);

            // Act
            containerRepository.Delete(container);

            var found = containerRepository.Get(container.Id);
            Assert.That(found, Is.Null);

            contentType = repository.Get(contentType.Id);
            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.ParentId, Is.EqualTo(-1));
        }
    }

    [Test]
    public void Can_Perform_Add_On_MediaTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var contentType = MediaTypeBuilder.CreateNewMediaType();
            repository.Save(contentType);

            var fetched = repository.Get(contentType.Id);

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
            Assert.That(contentType.Path, Does.Contain(","));
            Assert.That(contentType.SortOrder, Is.GreaterThan(0));

            TestHelper.AssertPropertyValuesAreEqual(contentType, fetched, ignoreProperties: new[] { "UpdateDate" });
        }
    }

    [Test]
    public void Can_Perform_Update_On_MediaTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var videoMediaType = MediaTypeBuilder.CreateNewMediaType();
            repository.Save(videoMediaType);

            // Act
            var mediaType = repository.Get(videoMediaType.Id);

            mediaType.Thumbnail = "Doc2.png";
            mediaType.PropertyGroups["media"].PropertyTypes.Add(
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "subtitle")
                {
                    Name = "Subtitle",
                    Description = "Optional Subtitle",
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = -88
                });
            repository.Save(mediaType);

            var dirty = ((MediaType)mediaType).IsDirty();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var mediaType = MediaTypeBuilder.CreateNewMediaType();
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var mediaType = repository.Get(1033); // File

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var mediaType = repository.Get(1033); // File

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var mediaTypes = repository.GetMany();
            var count =
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var allGuidIds = repository.GetMany().Select(x => x.Key).ToArray();

            // Act
            var mediaTypes = ((IReadRepository<Guid, IMediaType>)repository).GetMany(allGuidIds);

            var count =
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var exists = repository.Exists(1032); // Image

            // Assert
            Assert.That(exists, Is.True);
        }
    }

    [Test]
    public void Can_Update_MediaType_With_PropertyType_Removed()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var mediaType = MediaTypeBuilder.CreateNewMediaType();
            repository.Save(mediaType);

            // Act
            var mediaTypeV2 = repository.Get(mediaType.Id);
            mediaTypeV2.PropertyGroups["media"].PropertyTypes.Remove("title");
            repository.Save(mediaTypeV2);

            var mediaTypeV3 = repository.Get(mediaType.Id);

            // Assert
            Assert.That(mediaTypeV3.PropertyTypes.Any(x => x.Alias == "title"), Is.False);
            Assert.That(mediaTypeV2.PropertyGroups, Has.Count.EqualTo(mediaTypeV3.PropertyGroups.Count));
            Assert.That(mediaTypeV2.PropertyTypes.Count(), Is.EqualTo(mediaTypeV3.PropertyTypes.Count()));
        }
    }

    [Test]
    public void Can_Verify_PropertyTypes_On_Video_MediaType()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var mediaType = MediaTypeBuilder.CreateNewMediaType();

            repository.Save(mediaType);

            // Act
            var contentType = repository.Get(mediaType.Id);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public void Can_Verify_PropertyTypes_On_File_MediaType()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var contentType = repository.Get(1033); // File

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public void Get_By_Guid_Returns_Deep_Clone_Not_Cached_Instance()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);
            IMediaType mediaType = MediaTypeBuilder.CreateNewMediaType();
            repository.Save(mediaType);

            var first = repository.Get(mediaType.Key);
            var second = repository.Get(mediaType.Key);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second.Id, Is.EqualTo(first.Id));
            Assert.That(second, Is.Not.SameAs(first));
        }
    }

    [Test]
    public void Exists_By_Guid_Returns_Correct_Result()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);
            IMediaType mediaType = MediaTypeBuilder.CreateNewMediaType();
            repository.Save(mediaType);

            Assert.That(repository.Exists(mediaType.Key), Is.True);
            Assert.That(repository.Exists(Guid.NewGuid()), Is.False);
        }
    }

    private MediaTypeRepository CreateRepository(IScopeProvider provider) =>
        new((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<MediaTypeRepository>(), CommonRepository, LanguageRepository, ShortStringHelper, Mock.Of<IRepositoryCacheVersionService>(), IdKeyMap, Mock.Of<ICacheSyncService>());

    private EntityContainerRepository CreateContainerRepository(IScopeProvider provider) =>
        new((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<EntityContainerRepository>(), Constants.ObjectTypes.MediaTypeContainer, Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
}
