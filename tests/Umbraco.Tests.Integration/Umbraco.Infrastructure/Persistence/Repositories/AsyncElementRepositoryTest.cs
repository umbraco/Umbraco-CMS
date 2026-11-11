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
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AsyncElementRepositoryTest : UmbracoIntegrationTest
{
    private IContentType _elementType = null!;
    private IContentType _otherElementType = null!;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IEFCoreScopeAccessor<UmbracoDbContext> ScopeAccessor => GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

    private CountingDbCommandInterceptor CommandCounter => GetRequiredService<CountingDbCommandInterceptor>();

    [SetUp]
    public async Task SetUpData()
    {
        _elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);

        _otherElementType = ContentTypeBuilder.CreateSimpleElementType("otherElementType", "Other Element Type");
        await ContentTypeService.CreateAsync(_otherElementType, Constants.Security.SuperUserKey);
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

    private AsyncElementRepository CreateRepository() => CreateRepository(AppCaches.Disabled);

    private AsyncElementRepository CreateRepository(AppCaches appCaches) =>
        CreateRepository(appCaches, Mock.Of<IEventAggregator>());

    private AsyncElementRepository CreateRepository(AppCaches appCaches, IEventAggregator eventAggregator) => new(
        ScopeAccessor,
        appCaches,
        LoggerFactory,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IRelationRepository>(),
        GetRequiredService<IRelationTypeRepository>(),
        PropertyEditors,
        GetRequiredService<DataValueReferenceFactoryCollection>(),
        GetRequiredService<IDataTypeService>(),
        eventAggregator,
        Mock.Of<IRepositoryCacheVersionService>(),
        Mock.Of<ICacheSyncService>(),
        GetRequiredService<IContentTypeRepository>(),
        GetRequiredService<IIdKeyMap>(),
        GetRequiredService<ITagRepository>(),
        GetRequiredService<IJsonSerializer>());

    private DocumentRepository CreateDocumentRepository(AppCaches appCaches) => new(
        ScopeAccessor,
        appCaches,
        LoggerFactory,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IRelationRepository>(),
        GetRequiredService<IRelationTypeRepository>(),
        PropertyEditors,
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

    private void Publish(IElement element)
    {
        element.PublishCulture(CultureImpact.Invariant, DateTime.UtcNow, PropertyEditors);
        element.PublishedState = PublishedState.Publishing;
    }

    private async Task<ContentType> CreateVariantElementTypeAsync()
    {
        var variantElementType = (ContentType)ContentTypeBuilder.CreateSimpleElementType("variantElementType", "Variant Element Type");
        variantElementType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in variantElementType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(variantElementType, Constants.Security.SuperUserKey);
        return variantElementType;
    }

    private Task<bool> ReadPublishedFlagAsync(int nodeId) =>
        ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.Elements.Where(element => element.NodeId == nodeId).Select(element => element.Published).SingleAsync());

    [Test]
    public void Repository_IsResolvableFromTheContainer() =>
        Assert.That(GetRequiredService<IAsyncElementRepository>(), Is.InstanceOf<AsyncElementRepository>());

    [Test]
    public async Task SaveAsync_ThenGetAsync_ReturnsTheElementWithItsStructuralDataAndProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element saved = ElementBuilder.CreateSimpleElement(_elementType, "Element One");
        await repository.SaveAsync(saved, CancellationToken.None);

        IElement? element = await repository.GetAsync(saved.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element!.Id, Is.EqualTo(saved.Id));
            Assert.That(element.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(element.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(element.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(element.Name, Is.EqualTo("Element One"));
            Assert.That(element.VersionId, Is.Not.Zero);
            Assert.That(element.ContentTypeId, Is.EqualTo(_elementType.Id));
            Assert.That(element.Path, Is.Not.Empty);
            Assert.That(element.Properties.Count, Is.Not.Zero);
        });
    }

    [Test]
    public async Task SaveAsync_AssignsIdentity()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        await repository.SaveAsync(element, CancellationToken.None);
        scope.Complete();

        Assert.That(element.HasIdentity, Is.True);
    }

    [Test]
    public async Task SaveAsync_NamesWithAtSigns_PersistVerbatim()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element1 = ElementBuilder.CreateSimpleElement(_elementType, "test@umbraco.org");
        Element element2 = ElementBuilder.CreateSimpleElement(_elementType, "@lightgiants");
        await repository.SaveAsync(element1, CancellationToken.None);
        await repository.SaveAsync(element2, CancellationToken.None);

        IElement? result1 = await repository.GetAsync(element1.Key, CancellationToken.None);
        IElement? result2 = await repository.GetAsync(element2.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(element1.HasIdentity, Is.True);
            Assert.That(element2.HasIdentity, Is.True);
            Assert.That(result1?.Name, Is.EqualTo(element1.Name));
            Assert.That(result2?.Name, Is.EqualTo(element2.Name));
        });
    }

    [Test]
    public async Task SaveAsync_ResetsTheDirtyState()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        element.SetValue("title", "Dirty Title");
        Assert.That(element.IsDirty(), Is.True);

        await repository.SaveAsync(element, CancellationToken.None);
        Assert.That(element.IsDirty(), Is.False);

        IElement? reloaded = await repository.GetAsync(element.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(((Element)reloaded!).IsDirty(), Is.False);
    }

    [Test]
    public async Task SaveAsync_OfADraft_UpdatesNameAndPropertiesWithoutANewVersion()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        await repository.SaveAsync(element, CancellationToken.None);

        IElement content = (await repository.GetAsync(element.Key, CancellationToken.None))!;
        content.Name = "Updated Name";
        await repository.SaveAsync(content, CancellationToken.None);

        IElement updated = (await repository.GetAsync(element.Key, CancellationToken.None))!;
        Assert.Multiple(() =>
        {
            Assert.That(updated.Id, Is.EqualTo(content.Id));
            Assert.That(updated.Name, Is.EqualTo("Updated Name"));
            Assert.That(updated.VersionId, Is.EqualTo(content.VersionId));
            Assert.That(content.GetValue("title"), Is.EqualTo("This is the element title"));
        });

        content.SetValue("title", "Updated Title");
        await repository.SaveAsync(content, CancellationToken.None);

        updated = (await repository.GetAsync(element.Key, CancellationToken.None))!;
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(updated.GetValue("title"), Is.EqualTo("Updated Title"));
            Assert.That(updated.VersionId, Is.EqualTo(content.VersionId));
        });
    }

    [Test]
    public async Task SaveAsync_CachesTheElementWhereGetAsyncLooksForIt()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());

        Element element = ElementBuilder.CreateSimpleElement(_elementType, "Cache Key Alignment");
        await repository.SaveAsync(element, CancellationToken.None);

        CommandCounter.Enabled = true;
        CommandCounter.Reset();
        IElement? afterSave = await repository.GetAsync(element.Key, CancellationToken.None);
        var commandCount = CommandCounter.Count;
        CommandCounter.Enabled = false;
        scope.Complete();

        Assert.That(afterSave, Is.Not.Null);
        Assert.That(commandCount, Is.Zero, "the read after a save should be served from the entry the save wrote");
    }

    [Test]
    public async Task GetAsync_SecondCall_IsServedFromTheCache()
    {
        AppCaches appCaches = CreateRealAppCaches();
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(appCaches);

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        await repository.SaveAsync(element, CancellationToken.None);
        appCaches.IsolatedCaches.ClearCache<IElement>();

        CommandCounter.Enabled = true;
        CommandCounter.Reset();
        await repository.GetAsync(element.Key, CancellationToken.None);
        var firstCount = CommandCounter.Count;

        CommandCounter.Reset();
        await repository.GetAsync(element.Key, CancellationToken.None);
        var secondCount = CommandCounter.Count;
        CommandCounter.Enabled = false;
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(firstCount, Is.GreaterThan(0), "the first read after clearing the cache must hit the database");
            Assert.That(secondCount, Is.Zero, "the second read must be served from the cache");
        });
    }

    [Test]
    public async Task GetAllAsync_WithWarmCache_ReturnsAllElements()
    {
        AppCaches appCaches = CreateRealAppCaches();
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(appCaches);

        Element element1 = ElementBuilder.CreateSimpleElement(_elementType, "Element One");
        Element element2 = ElementBuilder.CreateSimpleElement(_elementType, "Element Two");
        await repository.SaveAsync(element1, CancellationToken.None);
        await repository.SaveAsync(element2, CancellationToken.None);
        appCaches.IsolatedCaches.ClearCache<IElement>();

        // warm the cache with one entry only, so a cache-first GetAll would miss the other
        await repository.GetAsync(element1.Key, CancellationToken.None);

        IElement[] all = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
        scope.Complete();

        Assert.That(all.Select(element => element.Key), Is.EquivalentTo(new[] { element1.Key, element2.Key }));
    }

    [Test]
    public async Task GetManyAsync_ReturnsOnlyTheRequestedElements()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element1 = ElementBuilder.CreateSimpleElement(_elementType, "Element One");
        Element element2 = ElementBuilder.CreateSimpleElement(_elementType, "Element Two");
        Element element3 = ElementBuilder.CreateSimpleElement(_elementType, "Element Three");
        await repository.SaveAsync(element1, CancellationToken.None);
        await repository.SaveAsync(element2, CancellationToken.None);
        await repository.SaveAsync(element3, CancellationToken.None);

        IElement[] elements = (await repository.GetManyAsync([element1.Key, element2.Key], CancellationToken.None)).ToArray();
        scope.Complete();

        Assert.That(elements.Select(element => element.Key), Is.EquivalentTo(new[] { element1.Key, element2.Key }));
    }

    [Test]
    public async Task GetAllAsync_AndGetManyAsync_ReturnEverySavedElement()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(ElementBuilder.CreateSimpleElement(_elementType, "Element One"), CancellationToken.None);
        await repository.SaveAsync(ElementBuilder.CreateSimpleElement(_elementType, "Element Two"), CancellationToken.None);

        IElement[] all = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
        IElement[] many = (await repository.GetManyAsync(all.Select(element => element.Key).ToArray(), CancellationToken.None)).ToArray();
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(all, Has.Length.EqualTo(2));
            Assert.That(many.Select(element => element.Key), Is.EquivalentTo(all.Select(element => element.Key)));
        });
    }

    [Test]
    public async Task ExistsAsync_ReflectsWhetherTheElementIsPersisted()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        await repository.SaveAsync(element, CancellationToken.None);

        var exists = await repository.ExistsAsync(element.Key, CancellationToken.None);
        var missing = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(exists, Is.True);
            Assert.That(missing, Is.False);
        });
    }

    [Test]
    public async Task CountAsync_ByContentTypeAlias_CountsOnlyThatType()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(ElementBuilder.CreateSimpleElement(_elementType, "Element One"), CancellationToken.None);
        await repository.SaveAsync(ElementBuilder.CreateSimpleElement(_elementType, "Element Two"), CancellationToken.None);
        await repository.SaveAsync(ElementBuilder.CreateSimpleElement(_otherElementType, "Element Three"), CancellationToken.None);

        var ofType = await repository.CountAsync(_elementType.Alias, CancellationToken.None);
        var total = await repository.CountAsync(CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(ofType, Is.EqualTo(2));
            Assert.That(total, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task GetManyAsync_RoundTripsPropertyValues()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var emptyElementType = ContentTypeBuilder.CreateBasicElementType("emptyElementType", "Empty Element Type");
        await ContentTypeService.CreateAsync(emptyElementType, Constants.Security.SuperUserKey);

        Element element1 = ElementBuilder.CreateSimpleElement(_elementType, "Element One");
        Element element2 = ElementBuilder.CreateBasicElement(emptyElementType);
        Element element3 = ElementBuilder.CreateSimpleElement(_elementType, "Element Three");
        await repository.SaveAsync(element1, CancellationToken.None);
        await repository.SaveAsync(element2, CancellationToken.None);
        await repository.SaveAsync(element3, CancellationToken.None);

        IElement[] result = (await repository.GetManyAsync([element1.Key, element2.Key, element3.Key], CancellationToken.None))
            .OrderBy(element => element.Id)
            .ToArray();
        scope.Complete();

        Assert.That(result.Select(element => element.Id), Is.EqualTo(new[] { element1.Id, element2.Id, element3.Id }));
        TestHelper.AssertPropertyValuesAreEqual(element1, result[0]);
        TestHelper.AssertPropertyValuesAreEqual(element2, result[1]);
        TestHelper.AssertPropertyValuesAreEqual(element3, result[2]);
    }

    [Test]
    public async Task DeleteAsync_RemovesTheElement()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        await repository.SaveAsync(element, CancellationToken.None);

        await repository.DeleteAsync(element, CancellationToken.None);
        IElement? afterDelete = await repository.GetAsync(element.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(afterDelete, Is.Null);
    }

    [Test]
    public async Task GetAsync_ByAnAssignedKey_ReturnsTheElement()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        element.Key = new Guid("A5C3A9D2-6B0E-4F1A-9E7C-3D8B2C1E4F60");
        await repository.SaveAsync(element, CancellationToken.None);

        IElement? result = await repository.GetAsync(new Guid("A5C3A9D2-6B0E-4F1A-9E7C-3D8B2C1E4F60"), CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(element.Id));
    }

    [Test]
    public async Task SaveAsync_PublishCreatesANewVersionAndADraftSaveDoesNot()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        var versions = new List<int>();

        IElement element1 = ElementBuilder.CreateSimpleElement(_elementType);

        // save = create the initial version
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        // publish = new edit version
        element1.SetValue("title", "title");
        Publish(element1);
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.Not.EqualTo(versions[^2]), "publishing must create a new edit version");
        Assert.That(element1.Published, Is.True);
        Assert.That(element1.PublishedState, Is.EqualTo(PublishedState.Published));
        Assert.That((await repository.GetAsync(element1.Key, CancellationToken.None))!.VersionId, Is.EqualTo(versions[^1]));
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.True);

        // save = update the current (draft) version
        element1.Name = "name-1";
        element1.SetValue("title", "title-1");
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.EqualTo(versions[^2]), "a draft save must not create a version");
        Assert.That(element1.Published, Is.True);
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.True);

        // unpublish = no impact on versions
        element1.PublishedState = PublishedState.Unpublishing;
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.EqualTo(versions[^2]));
        Assert.That(element1.Published, Is.False);
        Assert.That(element1.PublishedState, Is.EqualTo(PublishedState.Unpublished));
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.False);

        // save = update the current (draft) version
        element1.Name = "name-2";
        element1.SetValue("title", "title-2");
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.EqualTo(versions[^2]));
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.False);

        // publish = new version
        Publish(element1);
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.Not.EqualTo(versions[^2]));
        Assert.That(element1.Published, Is.True);
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.True);

        // save = update the current (draft) version
        element1.Name = "name-3";
        element1.SetValue("title", "title-3");
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.EqualTo(versions[^2]));
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.True);

        // publish = new version
        element1.Name = "name-4";
        element1.SetValue("title", "title-4");
        Publish(element1);
        await repository.SaveAsync(element1, CancellationToken.None);
        versions.Add(element1.VersionId);

        Assert.That(versions[^1], Is.Not.EqualTo(versions[^2]));
        Assert.That(element1.Published, Is.True);
        Assert.That(await ReadPublishedFlagAsync(element1.Id), Is.True);

        IElement[] allVersions = (await repository.GetAllVersionsAsync(element1.Key, CancellationToken.None)).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(allVersions, Has.Length.EqualTo(4));
            Assert.That(allVersions.All(version => version.PublishedVersionId == versions[^2]), Is.True, "every version reports the current published version");
            Assert.That(allVersions.Select(version => version.VersionId).Distinct().Count(), Is.EqualTo(4));
        });

        // get an older version
        IElement? older = await repository.GetVersionAsync(versions[^4], CancellationToken.None);
        Assert.That(older, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(older!.VersionId, Is.EqualTo(versions[^4]));
            Assert.That(element1.Name, Is.EqualTo("name-4"));
            Assert.That(element1.GetValue("title"), Is.EqualTo("title-4"));
            Assert.That(older.Name, Is.EqualTo("name-2"));
            Assert.That(older.GetValue("title"), Is.EqualTo("title-2"));
        });

        // all versions, most recent first
        int[] expectedVersions = versions.Distinct().Reverse().ToArray();
        Assert.That(allVersions.Select(version => version.VersionId), Is.EqualTo(expectedVersions));
        scope.Complete();
    }

    [Test]
    [LongRunning]
    public async Task GetAllAsync_AfterSavingAndPublishingEveryElement_ReturnsThemAll()
    {
        IElement[] result;
        using (var scope = NewScopeProvider.CreateScope())
        {
            var repository = CreateRepository();
            for (var i = 0; i < 4; i++)
            {
                await repository.SaveAsync(ElementBuilder.CreateSimpleElement(_elementType, "Element " + i), CancellationToken.None);
            }

            result = (await repository.GetAllAsync(CancellationToken.None)).ToArray();

            foreach (IElement element in result)
            {
                element.SetValue("title", element.GetValue<string>("title") + "x");
                await repository.SaveAsync(element, CancellationToken.None);
            }

            foreach (IElement element in result)
            {
                Publish(element);
                await repository.SaveAsync(element, CancellationToken.None);
            }

            scope.Complete();
        }

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repository = CreateRepository();
            IElement[] result2 = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
            scope.Complete();

            Assert.That(result2, Has.Length.EqualTo(result.Length));
        }
    }

    private async Task<(Element One, Element Two)> CreateTwoElementsAndOneOfAnotherTypeAsync(AsyncElementRepository repository)
    {
        Element element1 = ElementBuilder.CreateSimpleElement(_elementType, "Element One");
        Element element2 = ElementBuilder.CreateSimpleElement(_elementType, "Element Two");
        Element otherTypeElement = ElementBuilder.CreateSimpleElement(_otherElementType, "Element Three");
        await repository.SaveAsync(element1, CancellationToken.None);
        await repository.SaveAsync(element2, CancellationToken.None);
        await repository.SaveAsync(otherTypeElement, CancellationToken.None);
        return (element1, element2);
    }

    [Test]
    public async Task GetPagedOfContentTypesAsync_FirstPage_ReturnsTheFirstOfTheTypeOnly()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateTwoElementsAndOneOfAnotherTypeAsync(repository);

        PagedModel<IElement> result = await repository.GetPagedOfContentTypesAsync([_elementType.Key], 0, 1, Ordering.By("Name"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items.Select(element => element.Name), Is.EqualTo(new[] { "Element One" }));
        });
    }

    [Test]
    public async Task GetPagedOfContentTypesAsync_SecondPage_ReturnsTheSecondOfTheTypeOnly()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateTwoElementsAndOneOfAnotherTypeAsync(repository);

        PagedModel<IElement> result = await repository.GetPagedOfContentTypesAsync([_elementType.Key], 1, 1, Ordering.By("Name"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items.Select(element => element.Name), Is.EqualTo(new[] { "Element Two" }));
        });
    }

    [Test]
    public async Task GetPagedOfContentTypesAsync_SinglePage_ReturnsEveryElementOfTheType()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateTwoElementsAndOneOfAnotherTypeAsync(repository);

        PagedModel<IElement> result = await repository.GetPagedOfContentTypesAsync([_elementType.Key], 0, 2, Ordering.By("Name"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items.Select(element => element.Name), Is.EqualTo(new[] { "Element One", "Element Two" }));
        });
    }

    [Test]
    public async Task GetPagedOfContentTypesAsync_DescendingOrder_ReturnsTheLastOfTheTypeFirst()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateTwoElementsAndOneOfAnotherTypeAsync(repository);

        PagedModel<IElement> result = await repository.GetPagedOfContentTypesAsync([_elementType.Key], 0, 1, Ordering.By("Name", Direction.Descending), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items.Select(element => element.Name), Is.EqualTo(new[] { "Element Two" }));
        });
    }

    [Test]
    public async Task GetChildrenAsync_OrderedByCustomProperty_PagesInPropertyOrder()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element1 = ElementBuilder.CreateSimpleElement(_elementType, "Element One");
        element1.SetValue("title", "Bravo");
        Element element2 = ElementBuilder.CreateSimpleElement(_elementType, "Element Two");
        element2.SetValue("title", "Alpha");
        Element element3 = ElementBuilder.CreateSimpleElement(_elementType, "Element Three");
        element3.SetValue("title", "Charlie");
        await repository.SaveAsync(element1, CancellationToken.None);
        await repository.SaveAsync(element2, CancellationToken.None);
        await repository.SaveAsync(element3, CancellationToken.None);

        Ordering ordering = Ordering.By("title", isCustomField: true);
        PagedModel<IElement> firstPage = await repository.GetChildrenAsync(null, 0, 2, propertyAliases: null, ordering, CancellationToken.None);
        PagedModel<IElement> secondPage = await repository.GetChildrenAsync(null, 2, 2, propertyAliases: null, ordering, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.Total, Is.EqualTo(3));
            Assert.That(firstPage.Items.Select(element => element.Id), Is.EqualTo(new[] { element2.Id, element1.Id }));
            Assert.That(secondPage.Items.Select(element => element.Id), Is.EqualTo(new[] { element3.Id }));
        });
    }

    [Test]
    public async Task GetChildrenAsync_WithVariantAndInvariantElements_ResolvesEachAtItsOwnVariation()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var invariantElementType = (ContentType)ContentTypeBuilder.CreateSimpleElementType("invariantElementType", "Invariant Element Type");
        invariantElementType.Variations = ContentVariation.Nothing;
        foreach (IPropertyType propertyType in invariantElementType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Nothing;
        }

        await ContentTypeService.CreateAsync(invariantElementType, Constants.Security.SuperUserKey);

        var variantElementType = (ContentType)ContentTypeBuilder.CreateSimpleElementType("variantElementType", "Variant Element Type");
        variantElementType.Variations = ContentVariation.Culture;
        List<IPropertyType> propertyTypes = variantElementType.PropertyTypes.ToList();
        for (var i = 0; i < propertyTypes.Count; i++)
        {
            propertyTypes[i].Variations = i % 2 == 0 ? ContentVariation.Culture : ContentVariation.Nothing;
        }

        await ContentTypeService.CreateAsync(variantElementType, Constants.Security.SuperUserKey);

        for (var i = 0; i < 10; i++)
        {
            var isInvariant = i % 2 == 0;
            var name = (isInvariant ? "INV" : "VAR") + "_" + Guid.NewGuid();
            Element element = isInvariant
                ? ElementBuilder.CreateSimpleElement(invariantElementType, name)
                : ElementBuilder.CreateBasicElement(variantElementType);
            if (isInvariant is false)
            {
                element.SetCultureName(name, "en-US");
                element.SetValue("title", name + " Subpage", "en-US");
                element.SetValue("bodyText", "This is a subpage");
                element.SetValue("author", "John Doe", "en-US");
            }

            await repository.SaveAsync(element, CancellationToken.None);
        }

        PagedModel<IElement> result = await repository.GetChildrenAsync(null, 0, 20, propertyAliases: null, Ordering.By("UpdateDate"), CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(10));
        foreach (IElement element in result.Items)
        {
            var isInvariant = element.ContentType.Alias == "invariantElementType";
            var name = isInvariant ? element.Name : element.CultureInfos!["en-US"].Name;
            Assert.That(name, Does.StartWith(isInvariant ? "INV" : "VAR"));
            foreach (IProperty property in element.Properties)
            {
                var value = property.GetValue(property.PropertyType.Variations.VariesByNothing() ? null : "en-US");
                Assert.That(value, Is.Not.Null, $"{property.Alias} on {name}");
            }
        }
    }

    [Test]
    public async Task SaveAsync_OfAVariantElement_RoundTripsItsCultureVariationRows()
    {
        ContentType variantElementType = await CreateVariantElementTypeAsync();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateBasicElement(variantElementType);
        element.SetCultureName("English Name", "en-US");
        element.SetValue("title", "English Title", "en-US");
        await repository.SaveAsync(element, CancellationToken.None);
        int nodeId = element.Id;

        List<ElementCultureVariationDto> rows = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ElementCultureVariations.Where(variation => variation.NodeId == nodeId).ToListAsync());
        IElement? reloaded = await repository.GetAsync(element.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].Name, Is.EqualTo("English Name"));
            Assert.That(rows[0].Available, Is.True);
            Assert.That(rows[0].Published, Is.False);
            Assert.That(reloaded!.GetCultureName("en-US"), Is.EqualTo("English Name"));
            Assert.That(reloaded.GetValue("title", "en-US"), Is.EqualTo("English Title"));
        });
    }

    [Test]
    public async Task SaveAsync_PublishesAnElementRefreshNotification()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        var repository = CreateRepository(AppCaches.Disabled, eventAggregatorMock.Object);

        Element element = ElementBuilder.CreateSimpleElement(_elementType, "Notify Element");

        using var scope = NewScopeProvider.CreateScope();
        await repository.SaveAsync(element, CancellationToken.None);
        scope.Complete();

        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<ElementRefreshNotification>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_RemovesEveryElementRowAndLeavesDocumentsIntact()
    {
        var documentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        Content document = ContentBuilder.CreateSimpleContent(documentType, "A Document");
        await ContentService.SaveAsync(document, Constants.Security.SuperUserKey, null, CancellationToken.None);
        ContentType variantElementType = await CreateVariantElementTypeAsync();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateBasicElement(variantElementType);
        element.SetCultureName("English Name", "en-US");
        element.SetValue("title", "English Title", "en-US");
        element.PublishCulture(CultureImpact.Explicit("en-US", isDefault: true), DateTime.UtcNow, PropertyEditors);
        element.PublishedState = PublishedState.Publishing;
        await repository.SaveAsync(element, CancellationToken.None);
        int nodeId = element.Id;
        int documentId = document.Id;

        List<int> versionIds = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.Where(contentVersion => contentVersion.NodeId == nodeId).Select(contentVersion => contentVersion.Id).ToListAsync());
        (int elementRows, int versionRows, int cultureVariationRows) = await CountElementRowsAsync(nodeId, versionIds);
        Assert.Multiple(() =>
        {
            Assert.That(elementRows, Is.EqualTo(1), "precondition: the element row exists");
            Assert.That(versionRows, Is.EqualTo(2), "precondition: a published and a draft version exist");
            Assert.That(cultureVariationRows, Is.EqualTo(1), "precondition: the culture variation row exists");
        });

        await repository.DeleteAsync(element, CancellationToken.None);

        (elementRows, versionRows, cultureVariationRows) = await CountElementRowsAsync(nodeId, versionIds);
        (int nodeRows, int documentRows) = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(async db => (
            await db.Nodes.CountAsync(row => row.NodeId == nodeId),
            await db.Documents.CountAsync(row => row.NodeId == documentId)));
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(elementRows, Is.Zero);
            Assert.That(versionRows, Is.Zero);
            Assert.That(cultureVariationRows, Is.Zero);
            Assert.That(nodeRows, Is.Zero);
            Assert.That(documentRows, Is.EqualTo(1));
        });
    }

    private Task<(int ElementRows, int VersionRows, int CultureVariationRows)> CountElementRowsAsync(int nodeId, List<int> versionIds) =>
        ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(async db => (
            await db.Elements.CountAsync(row => row.NodeId == nodeId),
            await db.ElementVersions.CountAsync(row => versionIds.Contains(row.Id)),
            await db.ElementCultureVariations.CountAsync(row => row.NodeId == nodeId)));

    [Test]
    public async Task IsPathPublishedAsync_IsTrueOnlyForAPublishedElementOutsideTheRecycleBin()
    {
        var repository = CreateRepository();

        Element unpublished = ElementBuilder.CreateSimpleElement(_elementType, "Unpublished");
        Element published = ElementBuilder.CreateSimpleElement(_elementType, "Published");
        published.Published = true;
        Element publishedButTrashed = ElementBuilder.CreateSimpleElement(_elementType, "Published But Trashed");
        publishedButTrashed.Published = true;
        publishedButTrashed.Trashed = true;

        bool nullResult = await repository.IsPathPublishedAsync(null, CancellationToken.None);
        bool unpublishedResult = await repository.IsPathPublishedAsync(unpublished, CancellationToken.None);
        bool publishedButTrashedResult = await repository.IsPathPublishedAsync(publishedButTrashed, CancellationToken.None);
        bool publishedResult = await repository.IsPathPublishedAsync(published, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(nullResult, Is.False);
            Assert.That(unpublishedResult, Is.False);
            Assert.That(publishedButTrashedResult, Is.False);
            Assert.That(publishedResult, Is.True);
        });
    }

    [Test]
    public async Task DeleteAsync_ClearsTheElementStartNodeOfUserGroupsPointingAtIt()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element element = ElementBuilder.CreateSimpleElement(_elementType);
        await repository.SaveAsync(element, CancellationToken.None);
        int nodeId = element.Id;

        await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.UserGroups
                .Where(userGroup => userGroup.Alias == Constants.Security.AdminGroupAlias)
                .ExecuteUpdateAsync(setters => setters.SetProperty(userGroup => userGroup.StartElementId, nodeId)));

        await repository.DeleteAsync(element, CancellationToken.None);

        int? startElementId = await ScopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.UserGroups
                .Where(userGroup => userGroup.Alias == Constants.Security.AdminGroupAlias)
                .Select(userGroup => userGroup.StartElementId)
                .SingleAsync());
        scope.Complete();

        Assert.That(startElementId, Is.Null);
    }

    [Test]
    public async Task GetRecycleBinAsync_ReturnsTrashedElementsAtAnyDepthAndPagesThem()
    {
        IElementContainerService containerService = GetRequiredService<IElementContainerService>();
        EntityContainer trashedFolder = (await containerService.CreateAsync(null, "Trashed Folder", null, Constants.Security.SuperUserKey)).Result!;
        Attempt<EntityContainerOperationStatus> moveToBin = await containerService.MoveToRecycleBinAsync(trashedFolder.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToBin.Success, Is.True, moveToBin.Result.ToString());

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Element kept = ElementBuilder.CreateSimpleElement(_elementType, "Kept");
        Element trashedAtRoot = ElementBuilder.CreateSimpleElement(_elementType, "Trashed A");
        trashedAtRoot.ParentId = Constants.System.RecycleBinElement;
        trashedAtRoot.Trashed = true;
        Element trashedInFolder = ElementBuilder.CreateSimpleElement(_elementType, "Trashed B");
        trashedInFolder.ParentId = trashedFolder.Id;
        trashedInFolder.Trashed = true;
        await repository.SaveAsync(kept, CancellationToken.None);
        await repository.SaveAsync(trashedAtRoot, CancellationToken.None);
        await repository.SaveAsync(trashedInFolder, CancellationToken.None);

        IElement[] bin = (await repository.GetRecycleBinAsync(CancellationToken.None)).ToArray();
        PagedModel<IElement> firstPage = await repository.GetPagedRecycleBinAsync(0, 1, Ordering.By("name"), CancellationToken.None);
        PagedModel<IElement> secondPage = await repository.GetPagedRecycleBinAsync(1, 1, Ordering.By("name"), CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(bin.Select(element => element.Key), Is.EquivalentTo(new[] { trashedAtRoot.Key, trashedInFolder.Key }));
            Assert.That(bin.Single(element => element.Key == trashedInFolder.Key).ParentId, Is.EqualTo(trashedFolder.Id));
            Assert.That(firstPage.Total, Is.EqualTo(2));
            Assert.That(firstPage.Items.Select(element => element.Key), Is.EqualTo(new[] { trashedAtRoot.Key }));
            Assert.That(secondPage.Total, Is.EqualTo(2));
            Assert.That(secondPage.Items.Select(element => element.Key), Is.EqualTo(new[] { trashedInFolder.Key }));
        });
    }

    [Test]
    public async Task GetAsync_SharesNeitherRowsNorCacheEntriesWithDocuments()
    {
        AppCaches appCaches = CreateRealAppCaches();
        var documentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);

        using var scope = NewScopeProvider.CreateScope();
        var elementRepository = CreateRepository(appCaches);
        var documentRepository = CreateDocumentRepository(appCaches);

        Element element = ElementBuilder.CreateSimpleElement(_elementType, "Same Name");
        Content document = ContentBuilder.CreateSimpleContent(documentType, "Same Name");
        await elementRepository.SaveAsync(element, CancellationToken.None);
        await documentRepository.SaveAsync(document, CancellationToken.None);

        IElement? elementByOwnKey = await elementRepository.GetAsync(element.Key, CancellationToken.None);
        IContent? documentByElementKey = await documentRepository.GetAsync(element.Key, CancellationToken.None);
        IElement? elementByDocumentKey = await elementRepository.GetAsync(document.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(RepositoryCacheKeys.GetGuidKey<IElement>(), Is.Not.EqualTo(RepositoryCacheKeys.GetGuidKey<IContent>()));
            Assert.That(elementByOwnKey, Is.Not.Null);
            Assert.That(documentByElementKey, Is.Null);
            Assert.That(elementByDocumentKey, Is.Null);
        });
    }
}
