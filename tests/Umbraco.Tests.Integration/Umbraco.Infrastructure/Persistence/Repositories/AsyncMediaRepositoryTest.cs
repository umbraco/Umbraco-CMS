using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AsyncMediaRepositoryTest : UmbracoIntegrationTest
{
    private IMediaType _folderType = null!;
    private IMediaType _imageType = null!;
    private Media _testFolder = null!;
    private Media _testImage = null!;
    private Media _testFile = null!;

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IEFCoreScopeAccessor<UmbracoDbContext> ScopeAccessor => GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

    private CountingDbCommandInterceptor CommandCounter => GetRequiredService<CountingDbCommandInterceptor>();

    [SetUp]
    public async Task SetUpData()
    {
        _folderType = MediaTypeBuilder.CreateSimpleMediaType("testFolder", "Test Folder");
        await MediaTypeService.CreateAsync(_folderType, Constants.Security.SuperUserKey);

        _imageType = MediaTypeBuilder.CreateImageMediaType("testImage");
        await MediaTypeService.CreateAsync(_imageType, Constants.Security.SuperUserKey);

        _testFolder = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
        Assert.That(MediaService.Save(_testFolder).Success, Is.True);

        _testImage = MediaBuilder.CreateMediaImage(_imageType, _testFolder.Id);
        Assert.That(MediaService.Save(_testImage).Success, Is.True);

        _testFile = MediaBuilder.CreateMediaFile(_imageType, _testFolder.Id);
        Assert.That(MediaService.Save(_testFile).Success, Is.True);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<CountingDbCommandInterceptor>();
        ServiceDescriptor descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<UmbracoDbContext>));
        Func<IServiceProvider, object> originalFactory = descriptor.ImplementationFactory!;
        services.Remove(descriptor);
        services.AddSingleton<DbContextOptions<UmbracoDbContext>>(serviceProvider =>
        {
            var options = (DbContextOptions<UmbracoDbContext>)originalFactory(serviceProvider);
            return new DbContextOptionsBuilder<UmbracoDbContext>(options)
                .AddInterceptors(serviceProvider.GetRequiredService<CountingDbCommandInterceptor>())
                .Options;
        });
    }

    private static AppCaches CreateRealAppCaches() => new(
        new DeepCloneAppCache(new ObjectCacheAppCache()),
        new DictionaryAppCache(),
        new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache())));

    private AsyncMediaRepository CreateRepository() => CreateRepository(AppCaches.Disabled);

    private AsyncMediaRepository CreateRepository(AppCaches appCaches) =>
        CreateRepository(appCaches, Mock.Of<IEventAggregator>());

    private AsyncMediaRepository CreateRepository(AppCaches appCaches, IEventAggregator eventAggregator) => new(
        ScopeAccessor,
        appCaches,
        LoggerFactory,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IRelationRepository>(),
        GetRequiredService<IRelationTypeRepository>(),
        GetRequiredService<PropertyEditorCollection>(),
        GetRequiredService<DataValueReferenceFactoryCollection>(),
        GetRequiredService<IDataTypeService>(),
        eventAggregator,
        Mock.Of<IRepositoryCacheVersionService>(),
        Mock.Of<ICacheSyncService>(),
        GetRequiredService<IMediaTypeRepository>(),
        GetRequiredService<IIdKeyMap>(),
        GetRequiredService<ITagRepository>(),
        GetRequiredService<IJsonSerializer>(),
        GetRequiredService<MediaUrlGeneratorCollection>());

    private DocumentRepository CreateDocumentRepository(AppCaches appCaches) => new(
        ScopeAccessor,
        appCaches,
        LoggerFactory,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IRelationRepository>(),
        GetRequiredService<IRelationTypeRepository>(),
        GetRequiredService<PropertyEditorCollection>(),
        GetRequiredService<DataValueReferenceFactoryCollection>(),
        GetRequiredService<IDataTypeService>(),
        Mock.Of<IEventAggregator>(),
        Mock.Of<IRepositoryCacheVersionService>(),
        Mock.Of<ICacheSyncService>(),
        GetRequiredService<IContentTypeRepository>(),
        GetRequiredService<ITemplateRepository>(),
        GetRequiredService<IIdKeyMap>(),
        GetRequiredService<ITagRepository>(),
        GetRequiredService<IJsonSerializer>(),
        GetRequiredService<IShortStringHelper>());

    private Task<string?> ReadStoredPathAsync(int versionId) =>
        ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.MediaVersions.Where(mediaVersion => mediaVersion.Id == versionId).Select(mediaVersion => mediaVersion.Path).SingleAsync());

    private Task<DateTime> ReadVersionDateAsync(int versionId) =>
        ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.Where(contentVersion => contentVersion.Id == versionId).Select(contentVersion => contentVersion.VersionDate).SingleAsync());

    private Task<(int NodeRows, int VersionRows, int MediaVersionRows, int PropertyDataRows)> CountMediaRowsAsync(int nodeId, int versionId) =>
        ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(async db => (
            await db.Nodes.CountAsync(node => node.NodeId == nodeId),
            await db.ContentVersions.CountAsync(contentVersion => contentVersion.NodeId == nodeId),
            await db.MediaVersions.CountAsync(mediaVersion => mediaVersion.Id == versionId),
            await db.PropertyData.CountAsync(propertyData => propertyData.VersionId == versionId)));

    private async Task<int> CountCommandsAsync(Func<Task> action)
    {
        CommandCounter.Reset();
        CommandCounter.Enabled = true;
        await action();
        CommandCounter.Enabled = false;
        return CommandCounter.Count;
    }

    [Test]
    public void Repository_IsResolvableFromTheContainer() =>
        Assert.That(GetRequiredService<IAsyncMediaRepository>(), Is.InstanceOf<AsyncMediaRepository>());

    [Test]
    public async Task GetAsync_ForRootMedia_PopulatesNullParentKey()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia? folder = await repository.GetAsync(_testFolder.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(folder, Is.Not.Null);
        Assert.That(folder!.ParentKey, Is.Null, "the root node's own row carries RootSystemKey, which must not leak through the parent join");
    }

    [Test]
    public async Task GetAsync_ForNonRootMedia_ResolvesTheParentKeyWithoutAnExtraQuery()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia? image = null;
        IMedia[] pair = [];
        var singleCommands = await CountCommandsAsync(async () => image = await repository.GetAsync(_testImage.Key, CancellationToken.None));
        var pairCommands = await CountCommandsAsync(async () => pair = (await repository.GetManyAsync([_testImage.Key, _testFile.Key], CancellationToken.None)).ToArray());
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(image!.ParentKey, Is.EqualTo(_testFolder.Key));
            Assert.That(pair.Select(media => media.ParentKey), Is.All.EqualTo(_testFolder.Key));
            Assert.That(pairCommands, Is.EqualTo(singleCommands), "the parent key comes from the same join as the item, so reading two items costs no more commands than reading one");
        });
    }

    [Test]
    public async Task SaveAsync_ThenGetAsync_ReturnsTheMediaWithItsStructuralDataAndProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media saved = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);
        await repository.SaveAsync(saved, CancellationToken.None);

        IMedia? image = await repository.GetAsync(saved.Key, CancellationToken.None);
        int propertyDataRows = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.PropertyData.CountAsync(propertyData => propertyData.VersionId == saved.VersionId));
        scope.Complete();

        Assert.That(image, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved.HasIdentity, Is.True);
            Assert.That(propertyDataRows, Is.EqualTo(saved.Properties.Count), "one row per property, for the single version only");
            Assert.That(image!.Id, Is.EqualTo(saved.Id));
            Assert.That(image.Key, Is.EqualTo(saved.Key));
            Assert.That(image.SortOrder, Is.EqualTo(saved.SortOrder));
            Assert.That(image.Trashed, Is.False);
            Assert.That(image.CreatorId, Is.EqualTo(Constants.Security.UnknownUserId), "the builder leaves the creator unknown, which round-trips through a null user column");
            Assert.That(image.WriterId, Is.EqualTo(Constants.Security.UnknownUserId));
            Assert.That(image.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(image.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(image.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(image.Name, Is.EqualTo("Test Image"));
            Assert.That(image.VersionId, Is.EqualTo(saved.VersionId));
            Assert.That(image.ContentTypeId, Is.EqualTo(_imageType.Id));
            Assert.That(image.Path, Is.EqualTo($"{Constants.System.Root},{saved.Id}"));
            Assert.That(image.Level, Is.EqualTo(1));
            Assert.That(image.GetValue<string>(Constants.Conventions.Media.File), Is.EqualTo("/media/test-image.png"));
            Assert.That(image.GetValue<string>(Constants.Conventions.Media.Extension), Is.EqualTo("png"));
        });
        TestHelper.AssertPropertyValuesAreEqual(image, saved);
    }

    [Test]
    public async Task SaveAsync_ThenGetAsync_RoundTripsAKnownCreatorAndWriter()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media saved = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);
        saved.CreatorId = Constants.Security.SuperUserId;
        saved.WriterId = Constants.Security.SuperUserId;
        await repository.SaveAsync(saved, CancellationToken.None);

        IMedia? image = await repository.GetAsync(saved.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(image, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(image!.CreatorId, Is.EqualTo(Constants.Security.SuperUserId));
            Assert.That(image.WriterId, Is.EqualTo(Constants.Security.SuperUserId));
        });
    }

    [Test]
    public void SaveAsync_OfAnItemWithABlankName_IsRejected()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media blank = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
        blank.Name = " ";

        Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.SaveAsync(blank, CancellationToken.None),
            "a blank name is rejected, as for documents and elements, rather than auto-named as the NPoco repository does");
        Assert.That(blank.HasIdentity, Is.False);
    }

    [Test]
    public async Task SaveAsync_AssignsIdentityToEachSavedItem()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media file = MediaBuilder.CreateMediaFile(_imageType, Constants.System.Root);
        Media image = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);
        await repository.SaveAsync(file, CancellationToken.None);
        await repository.SaveAsync(image, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(file.HasIdentity, Is.True);
            Assert.That(image.HasIdentity, Is.True);
            Assert.That(file.Id, Is.Not.EqualTo(image.Id));
            Assert.That(file.Name, Is.EqualTo("Test File"));
            Assert.That(image.Name, Is.EqualTo("Test Image"));
            Assert.That(file.ContentTypeId, Is.EqualTo(_imageType.Id));
            Assert.That(image.ContentTypeId, Is.EqualTo(_imageType.Id));
        });
    }

    [Test]
    public async Task GetAsync_ReturnsACleanEntity()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia? image = await repository.GetAsync(_testImage.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(((Media)image!).IsDirty(), Is.False);
    }

    [Test]
    public async Task SaveAsync_OfARenamedItem_PersistsTheNewName()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia file = (await repository.GetAsync(_testFile.Key, CancellationToken.None))!;
        file.Name = "Test File Updated";
        await repository.SaveAsync(file, CancellationToken.None);

        IMedia? reloaded = await repository.GetAsync(_testFile.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(file.IsDirty(), Is.False);
            Assert.That(reloaded!.Id, Is.EqualTo(file.Id));
            Assert.That(reloaded.Name, Is.EqualTo("Test File Updated"));
            Assert.That(reloaded.VersionId, Is.EqualTo(_testFile.VersionId), "media keeps its single version on save");
        });
    }

    [Test]
    public async Task DeleteAsync_RemovesTheMedia()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia file = (await repository.GetAsync(_testFile.Key, CancellationToken.None))!;
        await repository.DeleteAsync(file, CancellationToken.None);

        IMedia? afterDelete = await repository.GetAsync(_testFile.Key, CancellationToken.None);
        var exists = await repository.ExistsAsync(_testFile.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(afterDelete, Is.Null);
            Assert.That(exists, Is.False);
        });
    }

    [Test]
    public async Task DeleteVersionsAsync_LeavesTheSingleVersionOfAMediaItemInPlace()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia file = (await repository.GetAsync(_testFile.Key, CancellationToken.None))!;
        var initialCount = (await repository.GetAllVersionsAsync(file.Key, CancellationToken.None)).Count();
        await repository.DeleteVersionsAsync(file.Key, DateTime.UtcNow.AddDays(1), CancellationToken.None);
        var afterFirstDelete = (await repository.GetAllVersionsAsync(file.Key, CancellationToken.None)).Count();

        file.Name = "Test File Updated";
        await repository.SaveAsync(file, CancellationToken.None);
        var afterUpdate = (await repository.GetAllVersionsAsync(file.Key, CancellationToken.None)).Count();
        await repository.DeleteVersionAsync(file.VersionId, CancellationToken.None);
        await repository.DeleteVersionsAsync(file.Key, DateTime.UtcNow.AddDays(1), CancellationToken.None);
        var afterSecondDelete = (await repository.GetAllVersionsAsync(file.Key, CancellationToken.None)).Count();
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(initialCount, Is.EqualTo(1));
            Assert.That(afterFirstDelete, Is.EqualTo(1));
            Assert.That(afterUpdate, Is.EqualTo(1), "a save of media does not create a version");
            Assert.That(afterSecondDelete, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetByLevelAsync_PagesTheItemsAtThatLevelInSortOrder()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IMedia> firstPage = await repository.GetByLevelAsync(2, 0, 1, Ordering.By("SortOrder"), CancellationToken.None);
        PagedModel<IMedia> secondPage = await repository.GetByLevelAsync(2, 1, 1, Ordering.By("SortOrder"), CancellationToken.None);
        PagedModel<IMedia> singlePage = await repository.GetByLevelAsync(2, 0, 2, Ordering.By("SortOrder"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.Total, Is.EqualTo(2));
            Assert.That(firstPage.Items.Select(media => media.Name), Is.EqualTo(new[] { "Test Image" }));
            Assert.That(secondPage.Total, Is.EqualTo(2));
            Assert.That(secondPage.Items.Select(media => media.Name), Is.EqualTo(new[] { "Test File" }));
            Assert.That(singlePage.Items.Select(media => media.Name), Is.EqualTo(new[] { "Test Image", "Test File" }));
        });
    }

    [Test]
    public async Task GetByLevelAsync_DescendingSortOrder_ReturnsTheLastItemFirst()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IMedia> page = await repository.GetByLevelAsync(2, 0, 1, Ordering.By("SortOrder", Direction.Descending), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(page.Total, Is.EqualTo(2));
            Assert.That(page.Items.Select(media => media.Name), Is.EqualTo(new[] { "Test File" }));
        });
    }

    [Test]
    public async Task GetByLevelAsync_OrderedByName_ReturnsTheAlphabeticallyFirstItemFirst()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IMedia> page = await repository.GetByLevelAsync(2, 0, 1, Ordering.By("Name"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(page.Total, Is.EqualTo(2));
            Assert.That(page.Items.Select(media => media.Name), Is.EqualTo(new[] { "Test File" }));
        });
    }

    [Test]
    public async Task GetPagedOfContentTypesAsync_ReturnsOnlyTheItemsOfThoseTypes()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        for (var i = 0; i < 10; i++)
        {
            await repository.SaveAsync(MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root), CancellationToken.None);
        }

        PagedModel<IMedia> folders = await repository.GetPagedOfContentTypesAsync([_folderType.Key], 0, 100, Ordering.By("SortOrder"), CancellationToken.None);
        PagedModel<IMedia> images = await repository.GetPagedOfContentTypesAsync([_imageType.Key], 0, 100, Ordering.By("SortOrder"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(folders.Total, Is.EqualTo(11));
            Assert.That(folders.Items.Select(media => media.ContentTypeId), Is.All.EqualTo(_folderType.Id));
            Assert.That(images.Total, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetManyAsync_ReturnsOnlyTheRequestedItems_AndGetAllAsyncReturnsEveryItem()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IMedia[] two = (await repository.GetManyAsync([_testImage.Key, _testFile.Key], CancellationToken.None)).ToArray();
        IMedia[] all = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
        IMedia[] allByKey = (await repository.GetManyAsync(all.Select(media => media.Key).ToArray(), CancellationToken.None)).ToArray();
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(two.Select(media => media.Key), Is.EquivalentTo(new[] { _testImage.Key, _testFile.Key }));
            Assert.That(all.Select(media => media.Key), Is.EquivalentTo(new[] { _testFolder.Key, _testImage.Key, _testFile.Key }));
            Assert.That(allByKey.Length, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task ExistsAsync_ReflectsWhetherTheMediaIsPersisted()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var exists = await repository.ExistsAsync(_testImage.Key, CancellationToken.None);
        var missing = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(exists, Is.True);
            Assert.That(missing, Is.False);
        });
    }

    [Test]
    public async Task CountAsync_AndCountChildrenAsync_CountTheItemsOfThisKindOnly()
    {
        var documentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        Content document = ContentBuilder.CreateSimpleContent(documentType, "A Document");
        await ContentService.SaveAsync(document, Constants.Security.SuperUserKey, null, CancellationToken.None);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var total = await repository.CountAsync(CancellationToken.None);
        var images = await repository.CountAsync(_imageType.Alias, CancellationToken.None);
        var atRoot = await repository.CountChildrenAsync(null, CancellationToken.None);
        var inFolder = await repository.CountChildrenAsync(_testFolder.Key, CancellationToken.None);
        var descendantsOfRoot = await repository.CountDescendantsAsync(null, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(total, Is.EqualTo(3), "the document at root is not counted");
            Assert.That(images, Is.EqualTo(2));
            Assert.That(atRoot, Is.EqualTo(1), "the document at root is not counted");
            Assert.That(inFolder, Is.EqualTo(2));
            Assert.That(descendantsOfRoot, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task SaveAsync_CachesTheMediaWhereGetAsyncLooksForIt()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());

        Media image = MediaBuilder.CreateMediaImage(_imageType, _testFolder.Id);
        await repository.SaveAsync(image, CancellationToken.None);

        IMedia? afterSave = null;
        var commandCount = await CountCommandsAsync(async () => afterSave = await repository.GetAsync(image.Key, CancellationToken.None));
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(afterSave, Is.Not.Null);
            Assert.That(commandCount, Is.Zero, "the read after a save is served from the entry the save wrote");
            Assert.That(image.ParentKey, Is.EqualTo(_testFolder.Key));
            Assert.That(afterSave!.ParentKey, Is.EqualTo(_testFolder.Key));
        });
    }

    [Test]
    public async Task GetAsync_SecondCall_IsServedFromTheCache()
    {
        AppCaches appCaches = CreateRealAppCaches();
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(appCaches);

        Media image = MediaBuilder.CreateMediaImage(_imageType, _testFolder.Id);
        await repository.SaveAsync(image, CancellationToken.None);
        appCaches.IsolatedCaches.ClearCache<IMedia>();

        var firstCount = await CountCommandsAsync(() => repository.GetAsync(image.Key, CancellationToken.None));
        var secondCount = await CountCommandsAsync(() => repository.GetAsync(image.Key, CancellationToken.None));
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(firstCount, Is.GreaterThan(0), "the first read after clearing the cache must hit the database");
            Assert.That(secondCount, Is.Zero, "the second read must be served from the cache");
        });
    }

    [Test]
    public async Task GetAllAsync_WithWarmCache_ReturnsEveryItem()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());

        Media image = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);
        await repository.SaveAsync(image, CancellationToken.None);

        IMedia[] all = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
        scope.Complete();

        Assert.That(all.Select(media => media.Key), Is.EquivalentTo(new[] { _testFolder.Key, _testImage.Key, _testFile.Key, image.Key }));
    }

    [Test]
    public async Task SaveAsync_OfAMovedItem_RehomesItUnderTheNewParent()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media otherFolder = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
        await repository.SaveAsync(otherFolder, CancellationToken.None);

        IMedia image = (await repository.GetAsync(_testImage.Key, CancellationToken.None))!;
        image.ParentId = otherFolder.Id;
        await repository.SaveAsync(image, CancellationToken.None);

        IMedia? reloaded = await repository.GetAsync(_testImage.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(image.ParentKey, Is.EqualTo(otherFolder.Key));
            Assert.That(image.Path, Is.EqualTo($"{otherFolder.Path},{image.Id}"));
            Assert.That(image.Level, Is.EqualTo(2));
            Assert.That(image.SortOrder, Is.Zero, "the first child of an empty folder takes the first sort order");
            Assert.That(reloaded!.ParentKey, Is.EqualTo(otherFolder.Key));
            Assert.That(reloaded.Path, Is.EqualTo(image.Path));
        });
    }

    [Test]
    public async Task SaveAsync_WritesTheFilePathToTheMediaVersionRow_AndUpdatesItOnSave()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media image = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);
        Media folder = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
        await repository.SaveAsync(image, CancellationToken.None);
        await repository.SaveAsync(folder, CancellationToken.None);
        var pathAfterInsert = await ReadStoredPathAsync(image.VersionId);
        var folderPath = await ReadStoredPathAsync(folder.VersionId);

        image.SetValue(Constants.Conventions.Media.File, "/media/renamed.png");
        await repository.SaveAsync(image, CancellationToken.None);
        var pathAfterUpdate = await ReadStoredPathAsync(image.VersionId);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(pathAfterInsert, Is.EqualTo("/media/test-image.png"));
            Assert.That(folderPath, Is.Null, "an item without a file stores no path");
            Assert.That(pathAfterUpdate, Is.EqualTo("/media/renamed.png"));
        });
    }

    [Test]
    public async Task GetMediaByPathAsync_FindsTheItemByItsStoredPathAndByAResizedRendition()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media inSizedFolder = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);
        inSizedFolder.SetValue(Constants.Conventions.Media.File, "/media/dir_1x1.v2/test-image.png");
        await repository.SaveAsync(inSizedFolder, CancellationToken.None);

        IMedia? byPath = await repository.GetMediaByPathAsync("/media/test-image.png", CancellationToken.None);
        IMedia? byResizedPath = await repository.GetMediaByPathAsync("/media/test-image_403x328.png", CancellationToken.None);
        IMedia? missing = await repository.GetMediaByPathAsync("/media/missing.png", CancellationToken.None);
        IMedia? sizeInFolderName = await repository.GetMediaByPathAsync("/media/dir_1x1.v2/test-image.png", CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(byPath?.Key, Is.EqualTo(_testImage.Key));
            Assert.That(byPath!.Properties.Count, Is.Not.Zero);
            Assert.That(byResizedPath?.Key, Is.EqualTo(_testImage.Key));
            Assert.That(missing, Is.Null);
            Assert.That(sizeInFolderName?.Key, Is.EqualTo(inSizedFolder.Key), "a size token in a directory name is not a rendition marker, so the path is looked up as given");
        });
    }

    [Test]
    public async Task DeleteAsync_ClearsTheMediaStartNodeOfUserGroupsPointingAtIt()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        int nodeId = _testFile.Id;

        await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.UserGroups
                .Where(userGroup => userGroup.Alias == Constants.Security.AdminGroupAlias)
                .ExecuteUpdateAsync(setters => setters.SetProperty(userGroup => userGroup.StartMediaId, nodeId)));

        IMedia file = (await repository.GetAsync(_testFile.Key, CancellationToken.None))!;
        await repository.DeleteAsync(file, CancellationToken.None);

        int? startMediaId = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.UserGroups
                .Where(userGroup => userGroup.Alias == Constants.Security.AdminGroupAlias)
                .Select(userGroup => userGroup.StartMediaId)
                .SingleAsync());
        scope.Complete();

        Assert.That(startMediaId, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_RemovesEveryRowOfTheItemAndLeavesOtherItemsAndDocumentsIntact()
    {
        var documentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        Content document = ContentBuilder.CreateSimpleContent(documentType, "A Document");
        await ContentService.SaveAsync(document, Constants.Security.SuperUserKey, null, CancellationToken.None);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        (int nodeRows, int versionRows, int mediaVersionRows, int propertyDataRows) = await CountMediaRowsAsync(_testImage.Id, _testImage.VersionId);
        (int NodeRows, int VersionRows, int MediaVersionRows, int PropertyDataRows) fileRowsBefore = await CountMediaRowsAsync(_testFile.Id, _testFile.VersionId);
        Assert.Multiple(() =>
        {
            Assert.That(nodeRows, Is.EqualTo(1), "precondition: the node row exists");
            Assert.That(versionRows, Is.EqualTo(1), "precondition: the single version exists");
            Assert.That(mediaVersionRows, Is.EqualTo(1), "precondition: the media version row exists");
            Assert.That(propertyDataRows, Is.GreaterThan(0), "precondition: the version has property rows");
            Assert.That((fileRowsBefore.NodeRows, fileRowsBefore.VersionRows, fileRowsBefore.MediaVersionRows), Is.EqualTo((1, 1, 1)), "precondition: the other item has its rows");
            Assert.That(fileRowsBefore.PropertyDataRows, Is.GreaterThan(0), "precondition: the other item has property rows");
        });

        IMedia image = (await repository.GetAsync(_testImage.Key, CancellationToken.None))!;
        await repository.DeleteAsync(image, CancellationToken.None);

        (nodeRows, versionRows, mediaVersionRows, propertyDataRows) = await CountMediaRowsAsync(_testImage.Id, _testImage.VersionId);
        (int NodeRows, int VersionRows, int MediaVersionRows, int PropertyDataRows) fileRowsAfter = await CountMediaRowsAsync(_testFile.Id, _testFile.VersionId);
        int documentRows = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.Documents.CountAsync(row => row.NodeId == document.Id));
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(nodeRows, Is.Zero);
            Assert.That(versionRows, Is.Zero);
            Assert.That(mediaVersionRows, Is.Zero);
            Assert.That(propertyDataRows, Is.Zero);
            Assert.That(fileRowsAfter, Is.EqualTo(fileRowsBefore));
            Assert.That(documentRows, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task SaveAsync_PublishesAMediaRefreshNotification_OnInsertAndOnUpdate()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        var repository = CreateRepository(AppCaches.Disabled, eventAggregatorMock.Object);

        Media image = MediaBuilder.CreateMediaImage(_imageType, Constants.System.Root);

        using var scope = NewScopeProvider.CreateScope();
        await repository.SaveAsync(image, CancellationToken.None);
        image.Name = "Renamed";
        await repository.SaveAsync(image, CancellationToken.None);
        scope.Complete();

#pragma warning disable CS0618 // Type or member is obsolete
        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<MediaRefreshNotification>()), Times.Exactly(2));
#pragma warning restore CS0618
    }

    [Test]
    public async Task SaveAsync_OfAnUnchangedItem_StillRunsTheUpdateFlow()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        var repository = CreateRepository(AppCaches.Disabled, eventAggregatorMock.Object);

        using var scope = NewScopeProvider.CreateScope();
        IMedia image = (await repository.GetAsync(_testImage.Key, CancellationToken.None))!;
        Assert.That(image.IsDirty(), Is.False, "precondition: nothing is dirty on a freshly read item");

        await repository.SaveAsync(image, CancellationToken.None);
        scope.Complete();

#pragma warning disable CS0618 // Type or member is obsolete
        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<MediaRefreshNotification>()), Times.Once, "media saves whatever is dirty, as its NPoco repository does");
#pragma warning restore CS0618
    }

    [Test]
    public async Task SaveAsync_PersistsTagRelationshipsOnAPlainSave()
    {
        IMediaType tagsType = new MediaTypeBuilder()
            .WithAlias("taggedMedia")
            .WithName("Tagged Media")
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithAlias("tags")
            .WithName("Tags")
            .WithDataTypeId(Constants.DataTypes.Tags)
            .Done()
            .Done()
            .Build();
        await MediaTypeService.CreateAsync(tagsType, Constants.Security.SuperUserKey);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media tagged = new MediaBuilder()
            .WithMediaType(tagsType)
            .WithName("Tagged")
            .WithParentId(Constants.System.Root)
            .AddPropertyData()
            .WithKeyValue("tags", "[\"red\",\"blue\"]")
            .Done()
            .Build();
        await repository.SaveAsync(tagged, CancellationToken.None);
        int nodeId = tagged.Id;

        List<string> tags = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.TagRelationships
                .Where(tagRelationship => tagRelationship.NodeId == nodeId)
                .Join(db.Tags, tagRelationship => tagRelationship.TagId, tag => tag.Id, (tagRelationship, tag) => tag.Text)
                .ToListAsync());
        scope.Complete();

        Assert.That(tags, Is.EquivalentTo(new[] { "red", "blue" }), "a plain save of media writes its tags; there is no publish step to defer them to");
    }

    [Test]
    public async Task SaveAsync_WithManySiblingsOfTheSameName_MakesEachNameUnique()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var folders = new List<Media>();
        for (var i = 0; i < 12; i++)
        {
            Media folder = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
            await repository.SaveAsync(folder, CancellationToken.None);
            folders.Add(folder);
        }

        scope.Complete();

        // The fixture's own "Test Folder" already sits at root, so the twelve new ones take (1) to (12).
        Assert.Multiple(() =>
        {
            Assert.That(folders.Select(folder => folder.Name).Append(_testFolder.Name).Distinct().Count(), Is.EqualTo(13));
            Assert.That(folders[0].Name, Is.EqualTo("Test Folder (1)"));
            Assert.That(folders[11].Name, Is.EqualTo("Test Folder (12)"));
        });
    }

    [Test]
    public async Task GetChildrenAsync_WithAnEmptyAliasList_PagesTheDirectChildrenInSortOrderWithoutProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media subFolder = MediaBuilder.CreateMediaFolder(_folderType, _testFolder.Id);
        await repository.SaveAsync(subFolder, CancellationToken.None);
        Media grandchild = MediaBuilder.CreateMediaImage(_imageType, subFolder.Id);
        await repository.SaveAsync(grandchild, CancellationToken.None);

        PagedModel<IMedia> all = await repository.GetChildrenAsync(_testFolder.Key, 0, 10, [], Ordering.By("sortOrder"), CancellationToken.None);
        PagedModel<IMedia> secondOfOne = await repository.GetChildrenAsync(_testFolder.Key, 1, 1, [], Ordering.By("sortOrder"), CancellationToken.None);
        PagedModel<IMedia> ofRoot = await repository.GetChildrenAsync(null, 0, 10, [], Ordering.By("sortOrder"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(all.Total, Is.EqualTo(3));
            Assert.That(all.Items.Select(media => media.Key), Is.EqualTo(new[] { _testImage.Key, _testFile.Key, subFolder.Key }), "direct children only, in sort order");
            Assert.That(all.Items.Select(media => media.Properties.Count), Is.All.Zero, "an empty alias list asks for no properties at all");
            Assert.That(all.Items.Select(media => media.ParentKey), Is.All.EqualTo(_testFolder.Key));
            Assert.That(secondOfOne.Total, Is.EqualTo(3));
            Assert.That(secondOfOne.Items.Select(media => media.Key), Is.EqualTo(new[] { _testFile.Key }));
            Assert.That(ofRoot.Items.Select(media => media.Key), Is.EqualTo(new[] { _testFolder.Key }), "a null parent key is the root of the media tree");
        });
    }

    [Test]
    public async Task GetDescendantsAsync_ReturnsEveryDepthInPathOrder_AndCanLeaveOutTrashedItems()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media subFolder = MediaBuilder.CreateMediaFolder(_folderType, _testFolder.Id);
        await repository.SaveAsync(subFolder, CancellationToken.None);
        Media grandchild = MediaBuilder.CreateMediaImage(_imageType, subFolder.Id);
        await repository.SaveAsync(grandchild, CancellationToken.None);
        Media trashedGrandchild = MediaBuilder.CreateMediaFile(_imageType, subFolder.Id);
        trashedGrandchild.Trashed = true;
        await repository.SaveAsync(trashedGrandchild, CancellationToken.None);

        PagedModel<IMedia> withTrashed = await repository.GetDescendantsAsync(_testFolder.Key, 0, 10, Ordering.By("path"), CancellationToken.None);
        PagedModel<IMedia> withoutTrashed = await repository.GetDescendantsAsync(_testFolder.Key, 0, 10, Ordering.By("path"), CancellationToken.None, includeTrashed: false);
        PagedModel<IMedia> page = await repository.GetDescendantsAsync(_testFolder.Key, 2, 2, Ordering.By("path"), CancellationToken.None, includeTrashed: false);
        PagedModel<IMedia> ofLeaf = await repository.GetDescendantsAsync(grandchild.Key, 0, 10, Ordering.By("path"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(withTrashed.Total, Is.EqualTo(5));
            Assert.That(withTrashed.Items.Select(media => media.Key), Is.EqualTo(new[] { _testImage.Key, _testFile.Key, subFolder.Key, grandchild.Key, trashedGrandchild.Key }));
            Assert.That(withoutTrashed.Total, Is.EqualTo(4));
            Assert.That(withoutTrashed.Items.Select(media => media.Key), Is.EqualTo(new[] { _testImage.Key, _testFile.Key, subFolder.Key, grandchild.Key }));
            Assert.That(page.Total, Is.EqualTo(4));
            Assert.That(page.Items.Select(media => media.Key), Is.EqualTo(new[] { subFolder.Key, grandchild.Key }));
            Assert.That(ofLeaf.Total, Is.Zero);
            Assert.That(ofLeaf.Items, Is.Empty);
        });
    }

    [Test]
    public async Task GetRecycleBinAsync_ReturnsTrashedMediaAtAnyDepthAndPagesThem()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media trashedFolder = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.RecycleBinMedia);
        trashedFolder.Name = "Trashed A";
        trashedFolder.Trashed = true;
        await repository.SaveAsync(trashedFolder, CancellationToken.None);

        Media trashedImage = MediaBuilder.CreateMediaImage(_imageType, trashedFolder.Id);
        trashedImage.Name = "Trashed B";
        trashedImage.Trashed = true;
        await repository.SaveAsync(trashedImage, CancellationToken.None);

        IMedia[] bin = (await repository.GetRecycleBinAsync(CancellationToken.None)).ToArray();
        PagedModel<IMedia> firstPage = await repository.GetPagedRecycleBinAsync(0, 1, Ordering.By("name"), CancellationToken.None);
        PagedModel<IMedia> secondPage = await repository.GetPagedRecycleBinAsync(1, 1, Ordering.By("name"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(bin.Select(media => media.Key), Is.EquivalentTo(new[] { trashedFolder.Key, trashedImage.Key }));
            Assert.That(bin.Single(media => media.Key == trashedImage.Key).ParentId, Is.EqualTo(trashedFolder.Id));
            Assert.That(firstPage.Total, Is.EqualTo(2));
            Assert.That(firstPage.Items.Select(media => media.Key), Is.EqualTo(new[] { trashedFolder.Key }));
            Assert.That(secondPage.Total, Is.EqualTo(2));
            Assert.That(secondPage.Items.Select(media => media.Key), Is.EqualTo(new[] { trashedImage.Key }));
        });
    }

    [Test]
    public async Task RecycleBinSmellsAsync_IsTrueOnlyWhenTheBinHoldsAnItem()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var beforeTrashing = await repository.RecycleBinSmellsAsync(CancellationToken.None);

        Media trashed = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.RecycleBinMedia);
        trashed.Trashed = true;
        await repository.SaveAsync(trashed, CancellationToken.None);

        var afterTrashing = await repository.RecycleBinSmellsAsync(CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(beforeTrashing, Is.False);
            Assert.That(afterTrashing, Is.True);
        });
    }

    [Test]
    public async Task RecycleBinSmellsAsync_CachesTheAnswerUnderTheMediaRecycleBinKey()
    {
        AppCaches appCaches = CreateRealAppCaches();
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(appCaches);

        bool? first = null;
        bool? second = null;
        bool? afterClear = null;
        var firstCount = await CountCommandsAsync(async () => first = await repository.RecycleBinSmellsAsync(CancellationToken.None));
        var secondCount = await CountCommandsAsync(async () => second = await repository.RecycleBinSmellsAsync(CancellationToken.None));

        Media trashed = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.RecycleBinMedia);
        trashed.Trashed = true;
        await repository.SaveAsync(trashed, CancellationToken.None);
        appCaches.RuntimeCache.ClearByKey(CacheKeys.MediaRecycleBinCacheKey);

        var afterClearCount = await CountCommandsAsync(async () => afterClear = await repository.RecycleBinSmellsAsync(CancellationToken.None));
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.False);
            Assert.That(firstCount, Is.GreaterThan(0), "the first answer comes from the database");
            Assert.That(second, Is.False);
            Assert.That(secondCount, Is.Zero, "the second answer is served from the cache");
            Assert.That(afterClear, Is.True, "clearing the media recycle bin key makes the next call look again");
            Assert.That(afterClearCount, Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task GetAncestorsAsync_ReturnsTheMediaAncestorsRootFirst()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media subFolder = MediaBuilder.CreateMediaFolder(_folderType, _testFolder.Id);
        await repository.SaveAsync(subFolder, CancellationToken.None);
        Media image = MediaBuilder.CreateMediaImage(_imageType, subFolder.Id);
        await repository.SaveAsync(image, CancellationToken.None);

        PagedModel<IMedia> ancestors = await repository.GetAncestorsAsync(image.Key, 0, 10, CancellationToken.None);
        PagedModel<IMedia> ofRootItem = await repository.GetAncestorsAsync(_testFolder.Key, 0, 10, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(ancestors.Total, Is.EqualTo(2));
            Assert.That(ancestors.Items.Select(media => media.Key), Is.EqualTo(new[] { _testFolder.Key, subFolder.Key }));
            Assert.That(ofRootItem.Total, Is.Zero);
        });
    }

    [Test]
    public async Task GetAsync_SharesNoRowsWithDocuments()
    {
        var documentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);

        using var scope = NewScopeProvider.CreateScope();
        var mediaRepository = CreateRepository();
        var documentRepository = CreateDocumentRepository(AppCaches.Disabled);

        Media media = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
        media.Name = "Same Name";
        Content document = ContentBuilder.CreateSimpleContent(documentType, "Same Name");
        await mediaRepository.SaveAsync(media, CancellationToken.None);
        await documentRepository.SaveAsync(document, CancellationToken.None);

        IMedia? mediaByOwnKey = await mediaRepository.GetAsync(media.Key, CancellationToken.None);
        IContent? documentByMediaKey = await documentRepository.GetAsync(media.Key, CancellationToken.None);
        IMedia? mediaByDocumentKey = await mediaRepository.GetAsync(document.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(mediaByOwnKey, Is.Not.Null);
            Assert.That(mediaByOwnKey!.Name, Is.EqualTo("Same Name"), "a media item and a document may share a name under root");
            Assert.That(documentByMediaKey, Is.Null);
            Assert.That(mediaByDocumentKey, Is.Null);
        });
    }

    [Test]
    public async Task SaveAsync_OfADescendantBeingMoved_WritesTheNodeRowOnly()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media otherFolder = MediaBuilder.CreateMediaFolder(_folderType, Constants.System.Root);
        await repository.SaveAsync(otherFolder, CancellationToken.None);

        // The image's own parent stays the same: it is its parent folder that moved under the other folder.
        IMedia image = (await repository.GetAsync(_testImage.Key, CancellationToken.None))!;
        DateTime versionDateBefore = await ReadVersionDateAsync(image.VersionId);
        image.Path = $"{otherFolder.Path},{_testFolder.Id},{image.Id}";
        image.Level = 3;
        image.SetValue(Constants.Conventions.Media.File, "/media/moved.png");
        await repository.SaveAsync(image, CancellationToken.None);

        DateTime versionDateAfter = await ReadVersionDateAsync(image.VersionId);
        string? storedFilePath = await ReadStoredPathAsync(image.VersionId);
        string storedPath = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.Nodes.Where(node => node.NodeId == image.Id).Select(node => node.Path).SingleAsync());
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(storedPath, Is.EqualTo($"{otherFolder.Path},{_testFolder.Id},{image.Id}"));
            Assert.That(versionDateAfter, Is.EqualTo(versionDateBefore), "a bulk-moved descendant skips the version, property data and tag steps");
            Assert.That(storedFilePath, Is.EqualTo("/media/test-image.png"), "a property edited alongside a bulk move is not written");
            Assert.That(image.IsDirty(), Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_OfANewItemWithAnExplicitSortOrder_TakesTheNextFreeOne()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Media image = MediaBuilder.CreateMediaImage(_imageType, _testFolder.Id);
        image.SortOrder = 7;
        await repository.SaveAsync(image, CancellationToken.None);

        IMedia? reloaded = await repository.GetAsync(image.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(image.SortOrder, Is.EqualTo(2), "the folder already holds two items at sort orders 0 and 1");
            Assert.That(reloaded!.SortOrder, Is.EqualTo(2));
        });
    }
}
