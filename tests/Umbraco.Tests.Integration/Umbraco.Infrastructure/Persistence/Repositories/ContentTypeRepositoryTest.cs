// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentTypeRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetUpData() => await CreateTestData();

    private ContentType _simpleContentType;
    private ContentType _textpageContentType;

    private FileSystems FileSystems => GetRequiredService<FileSystems>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IDocumentTypeContainerRepository DocumentTypeContainerRepository =>
        GetRequiredService<IDocumentTypeContainerRepository>();

    private IMediaTypeContainerRepository MediaTypeContainerRepository =>
        GetRequiredService<IMediaTypeContainerRepository>();

    private IMediaTypeRepository MediaTypeRepository => GetRequiredService<IMediaTypeRepository>();

    private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IUserGroupRepository UserGroupRepository => GetRequiredService<IUserGroupRepository>();

    private ITemplateRepository TemplateRepository => GetRequiredService<ITemplateRepository>();

    private ILanguageRepository LanguageRepository => GetRequiredService<ILanguageRepository>();

    private ContentTypeRepository ContentTypeRepository =>
        (ContentTypeRepository)GetRequiredService<IContentTypeRepository>();

    public async Task CreateTestData()
    {
        // Create and Save ContentType "umbTextpage" -> (_simpleContentType.Id)
        _simpleContentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: 0);

        await ContentTypeService.CreateAsync(_simpleContentType, Constants.Security.SuperUserKey);

        // Create and Save ContentType "textPage" -> (_textpageContentType.Id)
        _textpageContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: 0);
        await ContentTypeService.CreateAsync(_textpageContentType, Constants.Security.SuperUserKey);
    }

    // TODO: Add test to verify SetDefaultTemplates updates both AllowedTemplates and DefaultTemplate(id).

    [Test]
    public async Task Retrieval_By_Id_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        var contentType = _simpleContentType;
        database.EnableSqlCount = true;

        // Clear the isolated cache for IContentType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IContentType>();

        // Initial request by Id should hit the database.
        await repository.GetAsync(contentType.Id, CancellationToken.None);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache.
        await repository.GetAsync(contentType.Id, CancellationToken.None);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public async Task Retrieval_By_Key_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        var contentType = _simpleContentType;
        database.EnableSqlCount = true;

        // Clear the isolated cache for IContentType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IContentType>();

        // Initial request by key should hit the database.
        await repository.GetAsync(contentType.Key, CancellationToken.None);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache.
        await repository.GetAsync(contentType.Key, CancellationToken.None);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public async Task Retrieval_By_Key_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        var contentType = _simpleContentType;
        database.EnableSqlCount = true;

        // Clear the isolated cache for IContentType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IContentType>();

        // Initial request by ID should hit the database.
        await repository.GetAsync(contentType.Id, CancellationToken.None);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        await repository.GetAsync(contentType.Id, CancellationToken.None);
        Assert.AreEqual(0, database.SqlCount);

        await repository.GetAsync(contentType.Key, CancellationToken.None);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public async Task Retrieval_By_Id_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        var contentType = _simpleContentType;
        database.EnableSqlCount = true;

        // Clear the isolated cache for IContentType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IContentType>();

        // Initial request by key should hit the database.
        await repository.GetAsync(contentType.Key, CancellationToken.None);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        await repository.GetAsync(contentType.Key, CancellationToken.None);
        Assert.AreEqual(0, database.SqlCount);

        await repository.GetAsync(contentType.Id, CancellationToken.None);
        Assert.AreEqual(0, database.SqlCount);
    }

    private ContentTypeRepository CreateRepository(IScopeAccessor scopeAccessor, AppCaches? appCaches = null)
    {
        appCaches ??= AppCaches;

        var efCoreScopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var commonRepository =
            new ContentTypeCommonRepository(efCoreScopeAccessor, TemplateRepository, appCaches, ShortStringHelper);

        return new ContentTypeRepository(
            appCaches,
            LoggerFactory.CreateLogger<ContentTypeRepository>(),
            commonRepository,
            LanguageRepository,
            Mock.Of<IRepositoryCacheVersionService>(),
            IdKeyMap,
            Mock.Of<ICacheSyncService>(),
            efCoreScopeAccessor);
    }

    [Test]
    public async Task Maps_Templates_Correctly()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var runtimeSettingsMock = new Mock<IOptionsMonitor<RuntimeSettings>>();
            runtimeSettingsMock.Setup(x => x.CurrentValue).Returns(new RuntimeSettings());

            var templateRepo = new TemplateRepository(
                (IScopeAccessor)provider,
                AppCaches.Disabled,
                LoggerFactory.CreateLogger<TemplateRepository>(),
                LoggerFactory,
                FileSystems,
                ShortStringHelper,
                Mock.Of<IViewHelper>(),
                runtimeSettingsMock.Object,
                Mock.Of<IRepositoryCacheVersionService>(),
                Mock.Of<ICacheSyncService>());
            var repository = ContentTypeRepository;
            Template[] templates =
            {
                new(ShortStringHelper, "test1", "test1"), new(ShortStringHelper, "test2", "test2"),
                new(ShortStringHelper, "test3", "test3")
            };
            foreach (var template in templates)
            {
                templateRepo.Save(template);
            }

            var contentType = ContentTypeBuilder.CreateSimpleContentType();
            contentType.AllowedTemplates = new[] { templates[0], templates[1] };
            contentType.SetDefaultTemplate(templates[0]);
            await repository.SaveAsync(contentType, CancellationToken.None);

            // re-get
            var result = await repository.GetAsync(contentType.Id, CancellationToken.None);

            Assert.AreEqual(2, result.AllowedTemplates.Count());
            Assert.AreEqual(templates[0].Id, result.DefaultTemplate.Id);
        }
    }

    [Test]
    public async Task Can_Move()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var container1 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah1" };
            DocumentTypeContainerRepository.Save(container1);

            var container2 =
                new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah2", ParentId = container1.Id };
            DocumentTypeContainerRepository.Save(container2);

            var contentType = (IContentType)ContentTypeBuilder.CreateBasicContentType("asdfasdf");
            contentType.ParentId = container2.Id;
            await repository.SaveAsync(contentType, CancellationToken.None);

            // create a
            var contentType2 =
                (IContentType)new ContentType(ShortStringHelper, contentType, "hello") { Name = "Blahasdfsadf" };
            contentType.ParentId = contentType.Id;
            await repository.SaveAsync(contentType2, CancellationToken.None);

            var result = (await repository.MoveAsync(contentType, container1, CancellationToken.None)).ToArray();

            Assert.AreEqual(2, result.Count());

            // re-get
            contentType = await repository.GetAsync(contentType.Id, CancellationToken.None);
            contentType2 = await repository.GetAsync(contentType2.Id, CancellationToken.None);

            Assert.AreEqual(container1.Id, contentType.ParentId);
            Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType.Id).OriginalPath, contentType.Path);
            Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType2.Id).OriginalPath, contentType2.Path);
        }
    }

    [Test]
    public void Can_Create_Container()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah" };
            DocumentTypeContainerRepository.Save(container);

            Assert.That(container.Id, Is.GreaterThan(0));

            var found = DocumentTypeContainerRepository.Get(container.Id);
            Assert.IsNotNull(found);
        }
    }

    [Test]
    public void Can_Get_All_Containers()
    {
        EntityContainer container1, container2, container3;

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            container1 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container1" };
            DocumentTypeContainerRepository.Save(container1);
            container2 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container2" };
            DocumentTypeContainerRepository.Save(container2);
            container3 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container3" };
            DocumentTypeContainerRepository.Save(container3);

            Assert.That(container1.Id, Is.GreaterThan(0));
            Assert.That(container2.Id, Is.GreaterThan(0));
            Assert.That(container3.Id, Is.GreaterThan(0));

            var found1 = DocumentTypeContainerRepository.Get(container1.Id);
            Assert.IsNotNull(found1);
            var found2 = DocumentTypeContainerRepository.Get(container2.Id);
            Assert.IsNotNull(found2);
            var found3 = DocumentTypeContainerRepository.Get(container3.Id);
            Assert.IsNotNull(found3);
            var allContainers = DocumentTypeContainerRepository.GetMany();
            Assert.AreEqual(3, allContainers.Count());
        }
    }

    [Test]
    public void Can_Delete_Container()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah" };
            DocumentTypeContainerRepository.Save(container);

            // Act
            DocumentTypeContainerRepository.Delete(container);

            var found = DocumentTypeContainerRepository.Get(container.Id);
            Assert.IsNull(found);
        }
    }

    [Test]
    public async Task Can_Create_Container_Containing_Media_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
            MediaTypeContainerRepository.Save(container);

            var contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup", defaultTemplateId: 0);
            contentType.ParentId = container.Id;
            await repository.SaveAsync(contentType, CancellationToken.None);

            Assert.AreEqual(container.Id, contentType.ParentId);
        }
    }

    [Test]
    public void Can_Delete_Container_Containing_Media_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
            MediaTypeContainerRepository.Save(container);

            IMediaType contentType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup");
            contentType.ParentId = container.Id;
            MediaTypeRepository.Save(contentType);

            // Act
            MediaTypeContainerRepository.Delete(container);

            var found = MediaTypeContainerRepository.Get(container.Id);
            Assert.IsNull(found);

            contentType = MediaTypeRepository.Get(contentType.Id);
            Assert.IsNotNull(contentType);
            Assert.AreEqual(-1, contentType.ParentId);
        }
    }

    [Test]
    public async Task Can_Perform_Add_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            // Act
            var contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup");
            await ContentTypeRepository.SaveAsync(contentType, CancellationToken.None);

            var fetched = await ContentTypeRepository.GetAsync(contentType.Id, CancellationToken.None);

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
            Assert.That(contentType.PropertyTypes.All(x => x.HasIdentity), Is.True);
            Assert.That(contentType.Path.Contains(","), Is.True);
            Assert.That(contentType.SortOrder, Is.GreaterThan(0));

            Assert.That(contentType.PropertyGroups.ElementAt(0).Name == "testGroup", Is.True);
            var groupId = contentType.PropertyGroups.ElementAt(0).Id;
            Assert.That(contentType.PropertyTypes.All(x => x.PropertyGroupId.Value == groupId), Is.True);

            foreach (var propertyType in contentType.PropertyTypes)
            {
                Assert.AreNotEqual(propertyType.Key, Guid.Empty);
            }

            TestHelper.AssertPropertyValuesAreEqual(fetched, contentType, ignoreProperties: new[] { "DefaultTemplate", "AllowedTemplates", "UpdateDate", "HistoryCleanup" });
        }
    }

    [Test]
    public async Task Can_Perform_Update_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            contentType.Thumbnail = "Doc2.png";
            contentType.PropertyGroups["content"].PropertyTypes.Add(
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "subtitle")
                {
                    Name = "Subtitle",
                    Description = "Optional Subtitle",
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = -88,
                    LabelOnTop = true
                });
            await repository.SaveAsync(contentType, CancellationToken.None);

            var dirty = contentType.IsDirty();

            // Assert
            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(dirty, Is.False);
            Assert.That(contentType.Thumbnail, Is.EqualTo("Doc2.png"));
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
            Assert.That(contentType.PropertyTypes.Single(x => x.Alias == "subtitle").LabelOnTop, Is.True);
        }
    }

    [Test]
    public async Task Can_Perform_Delete_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: 0);
            await repository.SaveAsync(contentType, CancellationToken.None);

            var contentType2 = await repository.GetAsync(contentType.Id, CancellationToken.None);
            await repository.DeleteAsync(contentType2, CancellationToken.None);

            var exists = await repository.ExistsAsync(contentType.Id, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public async Task Can_Perform_Delete_With_Heirarchy_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var ctMain = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: 0);
            var ctChild1 = ContentTypeBuilder.CreateSimpleContentType("child1", "Child 1", ctMain, randomizeAliases: true, defaultTemplateId: 0);
            var ctChild2 = ContentTypeBuilder.CreateSimpleContentType("child2", "Child 2", ctChild1, randomizeAliases: true, defaultTemplateId: 0);

            await repository.SaveAsync(ctMain, CancellationToken.None);
            await repository.SaveAsync(ctChild1, CancellationToken.None);
            await repository.SaveAsync(ctChild2, CancellationToken.None);

            // Act
            var resolvedParent = await repository.GetAsync(ctMain.Id, CancellationToken.None);
            await repository.DeleteAsync(resolvedParent, CancellationToken.None);

            // Assert
            Assert.That(await repository.ExistsAsync(ctMain.Id, CancellationToken.None), Is.False);
            Assert.That(await repository.ExistsAsync(ctChild1.Id, CancellationToken.None), Is.False);
            Assert.That(await repository.ExistsAsync(ctChild2.Id, CancellationToken.None), Is.False);
        }
    }

    [Test]
    public async Task Can_Perform_Query_On_ContentTypeRepository_Sort_By_Name()
    {
        IContentType contentType;

        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);
            var child1 = ContentTypeBuilder.CreateSimpleContentType("abc", "abc", contentType, randomizeAliases: true, defaultTemplateId: 0);
            await repository.SaveAsync(child1, CancellationToken.None);
            var child3 = ContentTypeBuilder.CreateSimpleContentType("zyx", "zyx", contentType, randomizeAliases: true, defaultTemplateId: 0);
            await repository.SaveAsync(child3, CancellationToken.None);
            var child2 = ContentTypeBuilder.CreateSimpleContentType("a123", "a123", contentType, randomizeAliases: true, defaultTemplateId: 0);
            await repository.SaveAsync(child2, CancellationToken.None);

            scope.Complete();
        }

        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentTypes = (await repository.GetByParentIdAsync(contentType.Id, CancellationToken.None)).ToArray();

            // Assert
            Assert.That(contentTypes.Count(), Is.EqualTo(3));
            Assert.AreEqual("a123", contentTypes.ElementAt(0).Name);
            Assert.AreEqual("abc", contentTypes.ElementAt(1).Name);
            Assert.AreEqual("zyx", contentTypes.ElementAt(2).Name);
        }
    }

    [Test]
    public async Task Can_Perform_Get_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            // Assert
            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.Id, Is.EqualTo(_textpageContentType.Id));
        }
    }

    [Test]
    public async Task Can_Perform_Get_By_Guid_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);
            var childContentType = ContentTypeBuilder.CreateSimpleContentType("blah", "Blah", contentType, randomizeAliases: true, defaultTemplateId: 0);
            await repository.SaveAsync(childContentType, CancellationToken.None);

            // Act
            var result = await repository.GetAsync(childContentType.Key, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(childContentType.Id));
        }
    }

    [Test]
    public async Task Can_Perform_Get_By_Missing_Guid_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var result = await repository.GetAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentTypes = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
            var count =
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                    new { NodeObjectType = Constants.ObjectTypes.DocumentType });

            // Assert
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(count));
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_By_Guid_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var allGuidIds = (await repository.GetAllAsync(CancellationToken.None)).Select(x => x.Key).ToArray();

            // Act
            var contentTypes = (await repository.GetManyAsync(allGuidIds, CancellationToken.None)).ToArray();
            var count =
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                    new { NodeObjectType = Constants.ObjectTypes.DocumentType });

            // Assert
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(count));
        }
    }

    [Test]
    public async Task Can_Perform_Exists_On_ContentTypeRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var exists = await repository.ExistsAsync(_simpleContentType.Id, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.True);
        }
    }

    [Test]
    public async Task Can_Update_ContentType_With_PropertyType_Removed()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            // Act
            contentType.PropertyGroups["meta"].PropertyTypes.Remove("description");
            await repository.SaveAsync(contentType, CancellationToken.None);

            var result = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            // Assert
            Assert.That(result.PropertyTypes.Any(x => x.Alias == "description"), Is.False);
            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(result.PropertyGroups.Count));
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(result.PropertyTypes.Count()));
        }
    }

    [Test]
    public async Task Can_Verify_PropertyTypes_On_SimpleTextpage()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentType = await repository.GetAsync(_simpleContentType.Id, CancellationToken.None);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task Can_Verify_PropertyTypes_On_Textpage()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Act
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
            Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public async Task Can_Verify_PropertyType_With_No_Group()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(2));
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));

            // Act
            var urlAlias = new PropertyType(ShortStringHelper, "test", ValueStorageType.Nvarchar, "urlAlias")
            {
                Name = "Url Alias",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };

            var addedPropertyType = contentType.AddPropertyType(urlAlias);

            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(2));
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));

            await repository.SaveAsync(contentType, CancellationToken.None);

            // Assert
            var updated = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);
            Assert.That(addedPropertyType, Is.True);
            Assert.That(updated.PropertyGroups.Count, Is.EqualTo(2));
            Assert.That(updated.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(updated.PropertyTypes.Any(x => x.Alias == "urlAlias"), Is.True);
            Assert.That(updated.PropertyTypes.First(x => x.Alias == "urlAlias").PropertyGroupId, Is.Null);
        }
    }

    [Test]
    public async Task Can_Verify_AllowedChildContentTypes_On_ContentType()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var subpageContentType = ContentTypeBuilder.CreateSimpleContentType("umbSubpage", "Subpage");
            var simpleSubpageContentType =
                ContentTypeBuilder.CreateSimpleContentType("umbSimpleSubpage", "Simple Subpage");
            await repository.SaveAsync(subpageContentType, CancellationToken.None);
            await repository.SaveAsync(simpleSubpageContentType, CancellationToken.None);

            // Act
            var contentType = await repository.GetAsync(_simpleContentType.Id, CancellationToken.None);
            contentType.AllowedContentTypes = new List<ContentTypeSort>
            {
                new(subpageContentType.Key, 0, subpageContentType.Alias),
                new(simpleSubpageContentType.Key, 1, simpleSubpageContentType.Alias)
            };
            await repository.SaveAsync(contentType, CancellationToken.None);

            // Assert
            var updated = await repository.GetAsync(_simpleContentType.Id, CancellationToken.None);

            Assert.That(updated.AllowedContentTypes.Any(), Is.True);
            Assert.That(updated.AllowedContentTypes.Any(x => x.Alias == subpageContentType.Alias), Is.True);
            Assert.That(updated.AllowedContentTypes.Any(x => x.Alias == simpleSubpageContentType.Alias), Is.True);
        }
    }

    [Test]
    public async Task Can_Verify_Removal_Of_Used_PropertyType_From_ContentType()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            var subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
            DocumentRepository.Save(subpage);

            // Act
            contentType.RemovePropertyType("keywords");
            await repository.SaveAsync(contentType, CancellationToken.None);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "keywords"), Is.False);
            Assert.That(subpage.Properties.First(x => x.Alias == "description").GetValue(), Is.EqualTo("This is the meta description for a textpage"));
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Verify_Addition_Of_PropertyType_After_ContentType_Is_Used()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            var subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
            DocumentRepository.Save(subpage);

            // Act
            var propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
            propertyGroup.PropertyTypes.Add(
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "metaAuthor")
                {
                    Name = "Meta Author",
                    Description = string.Empty,
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = -88
                });
            await repository.SaveAsync(contentType, CancellationToken.None);

            // Assert
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
        }
    }

    [Test]
    public async Task Can_Verify_Usage_Of_New_PropertyType_On_Content()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            var subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
            DocumentRepository.Save(subpage);

            var propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
            propertyGroup.PropertyTypes.Add(
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "metaAuthor")
                {
                    Name = "Meta Author",
                    Description = string.Empty,
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = -88
                });
            await repository.SaveAsync(contentType, CancellationToken.None);

            // Act
            var content = DocumentRepository.Get(subpage.Id);
            content.SetValue("metaAuthor", "John Doe");
            DocumentRepository.Save(content);

            // Assert
            var updated = DocumentRepository.Get(subpage.Id);
            Assert.That(updated.GetValue("metaAuthor").ToString(), Is.EqualTo("John Doe"));
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
        }
    }

    [Test]
    public async Task
        Can_Verify_That_A_Combination_Of_Adding_And_Deleting_PropertyTypes_Doesnt_Cause_Issues_For_Content_And_ContentType()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var contentType = await repository.GetAsync(_textpageContentType.Id, CancellationToken.None);

            var subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
            DocumentRepository.Save(subpage);

            // Remove PropertyType
            contentType.RemovePropertyType("keywords");

            // Add PropertyType
            var propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
            propertyGroup.PropertyTypes.Add(
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "metaAuthor")
                {
                    Name = "Meta Author",
                    Description = string.Empty,
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = -88
                });
            await repository.SaveAsync(contentType, CancellationToken.None);

            // Act
            var content = DocumentRepository.Get(subpage.Id);
            content.SetValue("metaAuthor", "John Doe");
            DocumentRepository.Save(content);

            // Assert
            var updated = DocumentRepository.Get(subpage.Id);
            Assert.That(updated.GetValue("metaAuthor").ToString(), Is.EqualTo("John Doe"));
            Assert.That(updated.Properties.First(x => x.Alias == "description").GetValue(), Is.EqualTo("This is the meta description for a textpage"));

            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
            Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "keywords"), Is.False);
        }
    }

    [Test]
    public async Task Can_Verify_Content_Type_Has_Content_Nodes()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var contentTypeId = _textpageContentType.Id;
            var contentType = await repository.GetAsync(contentTypeId, CancellationToken.None);

            // Act
            var result = await repository.HasContentNodesAsync(contentTypeId, CancellationToken.None);

            var subpage = ContentBuilder.CreateTextpageContent(contentType, "Test Page 1", contentType.Id);
            DocumentRepository.Save(subpage);

            var result2 = await repository.HasContentNodesAsync(contentTypeId, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(result2, Is.True);
        }
    }

    [Test]
    public async Task Can_Update_Variation_Of_Element_Type_Property()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;
            var contentRepository = DocumentRepository;

            // Create elementType
            var elementType = new ContentType(ShortStringHelper, -1)
            {
                Alias = "elementType",
                Name = "Element type",
                Description = "Element type to use as compositions",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false,
                IsElement = true,
                Variations = ContentVariation.Nothing
            };

            var contentCollection = new PropertyTypeCollection(true)
            {
                new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext)
                {
                    Alias = "title",
                    Name = "Title",
                    Description = string.Empty,
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = Constants.DataTypes.Textbox,
                    LabelOnTop = true,
                    Variations = ContentVariation.Nothing
                }
            };
            elementType.PropertyGroups.Add(
                new PropertyGroup(contentCollection) { Name = "Content", Alias = "content", SortOrder = 1 });
            elementType.ResetDirtyProperties(false);
            elementType.SetDefaultTemplate(new Template(ShortStringHelper, "ElementType", "elementType"));
            await repository.SaveAsync(elementType, CancellationToken.None);

            // Create the basic "home" doc type that uses element type as comp
            var docType = new ContentType(ShortStringHelper, -1)
            {
                Alias = "home",
                Name = "Home",
                Description = "Home containing elementType",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false,
                Variations = ContentVariation.Nothing
            };
            var comp = new List<IContentTypeComposition>
            {
                elementType
            };
            docType.ContentTypeComposition = comp;
            await repository.SaveAsync(docType, CancellationToken.None);

            // Create "home" content
            var content = new Content("Home", -1, docType) { Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0 };
            object obj = new { title = "test title" };
            content.PropertyValues(obj);
            content.ResetDirtyProperties(false);
            contentRepository.Save(content);

            // Update variation on element type
            elementType.Variations = ContentVariation.Culture;
            elementType.PropertyTypes.First().Variations = ContentVariation.Culture;
            await repository.SaveAsync(elementType, CancellationToken.None);

            // Update variation on doc type
            docType.Variations = ContentVariation.Culture;
            await repository.SaveAsync(docType, CancellationToken.None);

            // Re fetch renewedContent and make sure that the culture has been set.
            var renewedContent = ContentService.GetById(content.Id);
            var hasCulture = renewedContent.Properties["title"].Values.First().Culture != null;
            Assert.That(hasCulture, Is.True);
        }
    }

    [Test]
    public async Task Can_Remove_Property_Value_Permissions_On_Removal_Of_Property_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            // Create, save and re-retrieve a content type and user group.
            IContentType contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: 0);
            await ContentTypeRepository.SaveAsync(contentType, CancellationToken.None);
            contentType = await ContentTypeRepository.GetAsync(contentType.Id, CancellationToken.None);

            var userGroup = CreateUserGroupWithGranularPermissions(contentType);

            // Remove property types and verify that the permission is removed from the user group.
            contentType.RemovePropertyType("author");
            await ContentTypeRepository.SaveAsync(contentType, CancellationToken.None);
            userGroup = UserGroupRepository.Get(userGroup.Id);
            Assert.AreEqual(3, userGroup.GranularPermissions.Count);

            contentType.RemovePropertyType("bodyText");
            await ContentTypeRepository.SaveAsync(contentType, CancellationToken.None);
            userGroup = UserGroupRepository.Get(userGroup.Id);
            Assert.AreEqual(2, userGroup.GranularPermissions.Count);

            contentType.RemovePropertyType("title");
            await ContentTypeRepository.SaveAsync(contentType, CancellationToken.None);
            userGroup = UserGroupRepository.Get(userGroup.Id);
            Assert.AreEqual(0, userGroup.GranularPermissions.Count);
        }
    }

    [Test]
    public async Task Can_Remove_Property_Value_Permissions_On_Removal_Of_Content_Type()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            // Create, save and re-retrieve a content type and user group.
            IContentType contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: 0);
            await ContentTypeRepository.SaveAsync(contentType, CancellationToken.None);
            contentType = await ContentTypeRepository.GetAsync(contentType.Id, CancellationToken.None);

            var userGroup = CreateUserGroupWithGranularPermissions(contentType);

            // Remove the content type and verify all permissions are removed from the user group.
            await ContentTypeRepository.DeleteAsync(contentType, CancellationToken.None);
            userGroup = UserGroupRepository.Get(userGroup.Id);
            Assert.AreEqual(0, userGroup.GranularPermissions.Count);
        }
    }

    private IUserGroup CreateUserGroupWithGranularPermissions(IContentType contentType)
    {
        DocumentPropertyValueGranularPermission CreatePermission(IPropertyType propertyType, string permission = "")
            => new()
            {
                Key = contentType.Key,
                Permission = propertyType.Key.ToString().ToLowerInvariant() + "|" + permission,
            };

        var titlePropertyType = contentType.PropertyTypes.Single(x => x.Alias == "title");
        var bodyTextPropertyType = contentType.PropertyTypes.Single(x => x.Alias == "bodyText");
        var authorPropertyType = contentType.PropertyTypes.Single(x => x.Alias == "author");

        var userGroup = new UserGroupBuilder()
            .WithGranularPermissions([
                CreatePermission(titlePropertyType, "Umb.Document.PropertyValue.Read"),
                CreatePermission(titlePropertyType, "Umb.Document.PropertyValue.Write"),
                CreatePermission(bodyTextPropertyType, "Umb.Document.PropertyValue.Read"),
                CreatePermission(authorPropertyType)
            ])
            .Build();
        UserGroupRepository.Save(userGroup);
        userGroup = UserGroupRepository.Get(userGroup.Id);

        Assert.AreEqual(4, userGroup.GranularPermissions.Count);
        return userGroup;
    }

    [Test]
    public async Task Get_By_Guid_Returns_Deep_Clone_Not_Cached_Instance()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var first = await repository.GetAsync(_simpleContentType.Key, CancellationToken.None);
            var second = await repository.GetAsync(_simpleContentType.Key, CancellationToken.None);

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(first.Id, second.Id);

            // Must be different object references (deep clones, not the cached original).
            Assert.AreNotSame(first, second);
        }
    }

    [Test]
    public async Task Get_By_Alias_Returns_Correct_ContentType()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Case-insensitive alias lookup.
            var result = await repository.GetAsync("UMBTEXTPAGE", CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(_simpleContentType.Id, result!.Id);
            Assert.AreEqual("umbTextpage", result.Alias);
        }
    }

    [Test]
    public async Task Get_By_Alias_Returns_Deep_Clone_Not_Cached_Instance()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var first = await repository.GetAsync("umbTextpage", CancellationToken.None);
            var second = await repository.GetAsync("umbTextpage", CancellationToken.None);

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(first!.Id, second!.Id);
            Assert.AreNotSame(first, second);
        }
    }

    [Test]
    public async Task Exists_By_Guid_Returns_True_For_Existing_Type()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var exists = await repository.ExistsAsync(_simpleContentType.Key, CancellationToken.None);

            Assert.IsTrue(exists);
        }
    }

    [Test]
    public async Task Exists_By_Guid_Returns_False_For_NonExisting_Type()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            var exists = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

            Assert.IsFalse(exists);
        }
    }

    [Test]
    public async Task Get_By_Guid_Mutation_Does_Not_Affect_Subsequent_Get()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = ContentTypeRepository;

            // Get a content type and mutate its name.
            var first = await repository.GetAsync(_simpleContentType.Key, CancellationToken.None);
            Assert.IsNotNull(first);
            var originalName = first!.Name;
            first.Name = "MUTATED_NAME_" + Guid.NewGuid();

            // A subsequent Get should return an unmodified clone.
            var second = await repository.GetAsync(_simpleContentType.Key, CancellationToken.None);
            Assert.IsNotNull(second);
            Assert.AreEqual(originalName, second!.Name, "Mutation of a returned entity should not affect the cached copy");
        }
    }
}
