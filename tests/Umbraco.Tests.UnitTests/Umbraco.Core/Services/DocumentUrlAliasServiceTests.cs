using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class DocumentUrlAliasServiceTests
{
    /// <summary>
    /// Creates a <see cref="DocumentUrlAliasService"/> wired up with mocked dependencies. The mocked
    /// <see cref="IScopeContext"/> immediately executes deferred <c>Enlist</c> callbacks so that the
    /// in-memory alias cache updates synchronously during the test.
    /// </summary>
    private static (DocumentUrlAliasService Service,
        Mock<IDocumentUrlAliasRepository> AliasRepository,
        Mock<IContentService> ContentService) CreateServiceWithMocks(
            ServerRole serverRole = ServerRole.Single,
            IEnumerable<ILanguage>? languages = null)
    {
        var effectiveLanguages = languages ?? Array.Empty<ILanguage>();

        var loggerMock = Mock.Of<ILogger<DocumentUrlAliasService>>();
        var aliasRepositoryMock = new Mock<IDocumentUrlAliasRepository>();
        var contentServiceMock = new Mock<IContentService>();

        var languageServiceMock = new Mock<ILanguageService>();
        languageServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(effectiveLanguages);
        languageServiceMock.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("en-US");

        // Return "1" (the service's CurrentRebuildValue constant) so ShouldRebuildAliases() returns false and
        // InitAsync skips the rebuild side-effect in tests that call InitAsync.
        var keyValueServiceMock = new Mock<IKeyValueService>();
        keyValueServiceMock.Setup(x => x.GetValue(DocumentUrlAliasService.RebuildKey)).Returns("1");

        var documentNavigationQueryServiceMock = Mock.Of<IDocumentNavigationQueryService>();

        var serverRoleAccessorMock = new Mock<IServerRoleAccessor>();
        serverRoleAccessorMock.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var scopeContextMock = new Mock<IScopeContext>();
        scopeContextMock.Setup(x => x.Enlist<bool>(
                It.IsAny<string>(),
                It.IsAny<Func<bool>>(),
                It.IsAny<Action<bool, bool>?>(),
                It.IsAny<int>()))
            .Returns((string _, Func<bool> creator, Action<bool, bool>? _, int _) => creator());

        var coreScopeMock = new Mock<ICoreScope>();
        coreScopeMock.Setup(x => x.Complete());

        var coreScopeProviderMock = new Mock<ICoreScopeProvider>();
        coreScopeProviderMock.Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(coreScopeMock.Object);
        coreScopeProviderMock.Setup(x => x.Context).Returns(scopeContextMock.Object);

        var service = new DocumentUrlAliasService(
            loggerMock,
            aliasRepositoryMock.Object,
            coreScopeProviderMock.Object,
            languageServiceMock.Object,
            keyValueServiceMock.Object,
            contentServiceMock.Object,
            documentNavigationQueryServiceMock,
            serverRoleAccessorMock.Object);

        return (service, aliasRepositoryMock, contentServiceMock);
    }

    /// <summary>
    /// Creates an invariant <see cref="IContent"/> mock whose alias property returns the supplied value when
    /// looked up via <see cref="IContentBase.GetValue"/>.
    /// </summary>
    private static IContent CreateInvariantContentWithAlias(Guid documentKey, string? aliasValue)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Variations).Returns(ContentVariation.Nothing);

        // ExtractAliasesFromDocumentAsync probes Properties for the alias property to decide whether it
        // varies by culture — an empty collection is sufficient for invariant content.
        var propertyCollectionMock = new Mock<IPropertyCollection>();
        propertyCollectionMock.Setup(x => x.GetEnumerator())
            .Returns(() => Enumerable.Empty<IProperty>().GetEnumerator());

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(documentKey);
        contentMock.Setup(x => x.Trashed).Returns(false);
        contentMock.Setup(x => x.Blueprint).Returns(false);
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentMock.Setup(x => x.Properties).Returns(propertyCollectionMock.Object);
        contentMock.Setup(x => x.GetValue<string>(
                Constants.Conventions.Content.UrlAlias,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Returns(aliasValue);

        return contentMock.Object;
    }

    /// <summary>
    /// Creates a variant <see cref="IContent"/> mock whose alias property itself varies by culture. The
    /// <paramref name="aliasesByCulture"/> map supplies the alias value for each ISO culture code; cultures
    /// not present in the map return null (simulating an unset value).
    /// </summary>
    private static IContent CreateVariantContentWithCultureVaryingAlias(
        Guid documentKey,
        IReadOnlyDictionary<string, string?> aliasesByCulture)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Variations).Returns(ContentVariation.Culture);

        var propertyTypeMock = new Mock<IPropertyType>();
        propertyTypeMock.Setup(x => x.Variations).Returns(ContentVariation.Culture);

        var aliasPropertyMock = new Mock<IProperty>();
        aliasPropertyMock.Setup(x => x.Alias).Returns(Constants.Conventions.Content.UrlAlias);
        aliasPropertyMock.Setup(x => x.PropertyType).Returns(propertyTypeMock.Object);

        var propertyCollectionMock = new Mock<IPropertyCollection>();
        propertyCollectionMock.Setup(x => x.GetEnumerator())
            .Returns(() => new[] { aliasPropertyMock.Object }.AsEnumerable().GetEnumerator());

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(documentKey);
        contentMock.Setup(x => x.Trashed).Returns(false);
        contentMock.Setup(x => x.Blueprint).Returns(false);
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentMock.Setup(x => x.Properties).Returns(propertyCollectionMock.Object);

        foreach ((string culture, string? value) in aliasesByCulture)
        {
            contentMock.Setup(x => x.GetValue<string>(
                    Constants.Conventions.Content.UrlAlias,
                    culture,
                    It.IsAny<string?>(),
                    It.IsAny<bool>()))
                .Returns(value);
        }

        return contentMock.Object;
    }

    /// <summary>
    /// Creates a simple <see cref="ILanguage"/> mock.
    /// </summary>
    private static ILanguage CreateMockLanguage(int id, string isoCode)
    {
        var languageMock = new Mock<ILanguage>();
        languageMock.Setup(x => x.Id).Returns(id);
        languageMock.Setup(x => x.IsoCode).Returns(isoCode);
        return languageMock.Object;
    }

    /// <summary>
    /// Creates a <see cref="DocumentUrlAliasService"/> pre-initialized with the supplied aliases loaded into
    /// its in-memory cache via <see cref="DocumentUrlAliasService.InitAsync"/>. Useful for testing the public
    /// lookup methods (<c>GetDocumentKeysByAliasAsync</c>, <c>GetAliasesAsync</c>, <c>HasAny</c>).
    /// </summary>
    private static async Task<DocumentUrlAliasService> CreateInitializedServiceWithAliases(
        IEnumerable<PublishedDocumentUrlAlias> seededAliases,
        IEnumerable<ILanguage> languages)
    {
        var (service, aliasRepositoryMock, _) = CreateServiceWithMocks(
            serverRole: ServerRole.Single,
            languages: languages);

        aliasRepositoryMock.Setup(x => x.GetAll()).Returns(seededAliases);

        await service.InitAsync(forceEmpty: false, CancellationToken.None);

        return service;
    }

    #region CreateOrUpdateAliasesAsync Tests

    /// <summary>
    /// For invariant content the alias property value is stored once, with <c>NullableLanguageId = null</c>,
    /// irrespective of how many languages are configured.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_InvariantContent_SavesAliasWithNullLanguageId()
    {
        // Arrange
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
            CreateMockLanguage(2, "fr-FR"),
        };
        var (service, aliasRepositoryMock, contentServiceMock) =
            CreateServiceWithMocks(languages: languages);

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, "my-alias"));

        List<PublishedDocumentUrlAlias>? savedAliases = null;
        aliasRepositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()))
            .Callback<IEnumerable<PublishedDocumentUrlAlias>>(aliases => savedAliases = aliases.ToList());

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        Assert.That(savedAliases, Is.Not.Null);
        Assert.That(savedAliases, Has.Count.EqualTo(1));
        Assert.That(savedAliases![0].DocumentKey, Is.EqualTo(documentKey));
        Assert.That(savedAliases[0].NullableLanguageId, Is.Null);
        Assert.That(savedAliases[0].Alias, Is.EqualTo("my-alias"));
    }

    /// <summary>
    /// The alias property may contain multiple comma-separated aliases. Each must be persisted as its own
    /// row sharing the same document key and (for invariant content) a null language id.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_InvariantContent_WithCommaSeparatedAliases_SavesOneRowPerAlias()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks();

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, "alias-one, alias-two, alias-three"));

        List<PublishedDocumentUrlAlias>? savedAliases = null;
        aliasRepositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()))
            .Callback<IEnumerable<PublishedDocumentUrlAlias>>(aliases => savedAliases = aliases.ToList());

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        Assert.That(savedAliases, Is.Not.Null);
        Assert.That(
            savedAliases!.Select(x => x.Alias),
            Is.EquivalentTo(new[] { "alias-one", "alias-two", "alias-three" }));
        Assert.That(
            savedAliases.All(x => x.NullableLanguageId is null),
            Is.True,
            "All rows from an invariant content must share a null language id.");
    }

    /// <summary>
    /// Aliases must be normalized before persistence: trimmed, stripped of leading/trailing slashes, and
    /// lowercased. This matches <c>IDocumentUrlAliasService.NormalizeAlias</c> so lookups round-trip.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_NormalizesAlias_BeforeSaving()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks();

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, "  /Some-Mixed-Case/  "));

        List<PublishedDocumentUrlAlias>? savedAliases = null;
        aliasRepositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()))
            .Callback<IEnumerable<PublishedDocumentUrlAlias>>(aliases => savedAliases = aliases.ToList());

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        Assert.That(savedAliases, Is.Not.Null);
        Assert.That(savedAliases, Has.Count.EqualTo(1));
        Assert.That(savedAliases![0].Alias, Is.EqualTo("some-mixed-case"));
    }

    /// <summary>
    /// For variant content whose alias property itself varies by culture, one row is saved per language —
    /// each tagged with that language's id. Cultures with no alias value contribute no rows.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_VariantContent_WithVariantAliasProperty_SavesOneRowPerLanguage()
    {
        // Arrange
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
            CreateMockLanguage(2, "fr-FR"),
        };
        var (service, aliasRepositoryMock, contentServiceMock) =
            CreateServiceWithMocks(languages: languages);

        var documentKey = Guid.NewGuid();
        var aliasesByCulture = new Dictionary<string, string?>
        {
            { "en-US", "english-alias" },
            { "fr-FR", "alias-francais" },
        };
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateVariantContentWithCultureVaryingAlias(documentKey, aliasesByCulture));

        List<PublishedDocumentUrlAlias>? savedAliases = null;
        aliasRepositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()))
            .Callback<IEnumerable<PublishedDocumentUrlAlias>>(aliases => savedAliases = aliases.ToList());

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        Assert.That(savedAliases, Is.Not.Null);
        Assert.That(savedAliases, Has.Count.EqualTo(2));

        var english = savedAliases!.Single(x => x.NullableLanguageId == 1);
        var french = savedAliases.Single(x => x.NullableLanguageId == 2);
        Assert.That(english.Alias, Is.EqualTo("english-alias"));
        Assert.That(french.Alias, Is.EqualTo("alias-francais"));
    }

    /// <summary>
    /// When the document cannot be found the service must clear any existing cache entries for that key
    /// (via the deferred <c>RemoveFromCacheDeferred</c>) and must not touch the repository.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_MissingDocument_DoesNotCallRepository()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks();

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey)).Returns((IContent?)null);

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Never);
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    /// <summary>
    /// Trashed content has no publicly-addressable URLs, so no aliases should be persisted (or deleted —
    /// the deferred cache cleanup handles any previously-cached entries).
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_TrashedDocument_DoesNotCallRepository()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks();

        var documentKey = Guid.NewGuid();
        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(documentKey);
        contentMock.Setup(x => x.Trashed).Returns(true);
        contentMock.Setup(x => x.Blueprint).Returns(false);
        contentServiceMock.Setup(x => x.GetById(documentKey)).Returns(contentMock.Object);

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Never);
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    /// <summary>
    /// Blueprints are templates — they never resolve to a URL, so no aliases should be persisted for them.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_BlueprintDocument_DoesNotCallRepository()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks();

        var documentKey = Guid.NewGuid();
        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(documentKey);
        contentMock.Setup(x => x.Trashed).Returns(false);
        contentMock.Setup(x => x.Blueprint).Returns(true);
        contentServiceMock.Setup(x => x.GetById(documentKey)).Returns(contentMock.Object);

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Never);
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    /// <summary>
    /// On a subscriber the scheduling publisher has already written the aliases, so the subscriber must not
    /// re-persist them — doing so crashes on a read-only database (issue #22570). This covers the path where
    /// the document has aliases and would otherwise hit <c>Save</c>.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_OnSubscriber_WithAliases_DoesNotCallSave()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks(ServerRole.Subscriber);

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, "my-alias"));

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Never,
            "Subscribers must not persist URL aliases — the publisher already has.");
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    /// <summary>
    /// Regression guard for the subscriber guard: Single and SchedulingPublisher roles must still persist
    /// URL aliases as they did before the fix.
    /// </summary>
    [TestCase(ServerRole.Single)]
    [TestCase(ServerRole.SchedulingPublisher)]
    public async Task CreateOrUpdateAliasesAsync_OnSingleOrPublisher_WithAliases_CallsSave(ServerRole role)
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks(role);

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, "my-alias"));

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Once,
            $"The {role} role must continue to persist URL aliases.");
    }

    /// <summary>
    /// Covers the alternate branch of the guard: when a document has no alias value the
    /// <see cref="IDocumentUrlAliasRepository.DeleteByDocumentKey"/> path must also be skipped on subscribers.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_OnSubscriber_WithNoAliases_DoesNotCallDeleteByDocumentKey()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks(ServerRole.Subscriber);

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, aliasValue: null));

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never,
            "Subscribers must not delete URL aliases.");
    }

    /// <summary>
    /// Regression guard: the Single role must continue to delete stale aliases when a document no longer
    /// has any alias value.
    /// </summary>
    [Test]
    public async Task CreateOrUpdateAliasesAsync_OnSingle_WithNoAliases_CallsDeleteByDocumentKey()
    {
        // Arrange
        var (service, aliasRepositoryMock, contentServiceMock) = CreateServiceWithMocks(ServerRole.Single);

        var documentKey = Guid.NewGuid();
        contentServiceMock.Setup(x => x.GetById(documentKey))
            .Returns(CreateInvariantContentWithAlias(documentKey, aliasValue: null));

        // Act
        await service.CreateOrUpdateAliasesAsync(documentKey);

        // Assert
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Once);
    }

    #endregion

    #region DeleteAliasesFromCacheAsync Tests

    /// <summary>
    /// <see cref="DocumentUrlAliasService.DeleteAliasesFromCacheAsync"/> only clears in-memory entries; it
    /// must never issue database writes. This matches the documented contract and keeps the method safe to
    /// call on subscribers.
    /// </summary>
    [Test]
    public async Task DeleteAliasesFromCacheAsync_DoesNotCallRepository()
    {
        // Arrange
        var (service, aliasRepositoryMock, _) = CreateServiceWithMocks();

        // Act
        await service.DeleteAliasesFromCacheAsync(new[] { Guid.NewGuid(), Guid.NewGuid() });

        // Assert
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Never);
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    #endregion

    #region RebuildAllAliasesAsync Tests

    /// <summary>
    /// <see cref="DocumentUrlAliasService.RebuildAllAliasesAsync"/> is a heavy, fully-writing operation.
    /// On a subscriber it must short-circuit before touching the database at all.
    /// </summary>
    [Test]
    public async Task RebuildAllAliasesAsync_OnSubscriber_IsNoOp()
    {
        // Arrange
        var (service, aliasRepositoryMock, _) = CreateServiceWithMocks(ServerRole.Subscriber);

        // Act
        await service.RebuildAllAliasesAsync();

        // Assert — none of the read or write side-effects should have fired.
        aliasRepositoryMock.Verify(x => x.GetAllDocumentUrlAliases(), Times.Never);
        aliasRepositoryMock.Verify(
            x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()),
            Times.Never);
        aliasRepositoryMock.Verify(
            x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    #endregion

    #region Lookup Tests

    /// <summary>
    /// After the service is initialized from persisted aliases, looking up a document key by its alias must
    /// return the matching document. Covers the common invariant-content lookup path.
    /// </summary>
    [Test]
    public async Task GetDocumentKeysByAliasAsync_Returns_MatchingDocumentKey()
    {
        // Arrange
        var documentKey = Guid.NewGuid();
        var seeded = new[]
        {
            new PublishedDocumentUrlAlias
            {
                DocumentKey = documentKey,
                NullableLanguageId = null,
                Alias = "my-alias",
            },
        };
        var languages = new List<ILanguage> { CreateMockLanguage(1, "en-US") };

        var service = await CreateInitializedServiceWithAliases(seeded, languages);

        // Act
        var result = (await service.GetDocumentKeysByAliasAsync("my-alias", culture: null)).ToList();

        // Assert
        Assert.That(result, Is.EqualTo(new[] { documentKey }));
    }

    /// <summary>
    /// Lookups by alias are case-insensitive and tolerate leading slashes in the input (they are normalized
    /// before lookup). Guards against regressions in <c>NormalizeAlias</c>.
    /// </summary>
    [Test]
    public async Task GetDocumentKeysByAliasAsync_IsCaseInsensitive_AndTrimsSlashes()
    {
        // Arrange
        var documentKey = Guid.NewGuid();
        var seeded = new[]
        {
            new PublishedDocumentUrlAlias
            {
                DocumentKey = documentKey,
                NullableLanguageId = null,
                Alias = "my-alias",
            },
        };
        var languages = new List<ILanguage> { CreateMockLanguage(1, "en-US") };

        var service = await CreateInitializedServiceWithAliases(seeded, languages);

        // Act
        var result = (await service.GetDocumentKeysByAliasAsync("/My-Alias/", culture: null)).ToList();

        // Assert
        Assert.That(result, Is.EqualTo(new[] { documentKey }));
    }

    /// <summary>
    /// Given a document key, the service must return all aliases associated with it.
    /// </summary>
    [Test]
    public async Task GetAliasesAsync_Returns_AliasesForDocument()
    {
        // Arrange
        var documentKey = Guid.NewGuid();
        var seeded = new[]
        {
            new PublishedDocumentUrlAlias { DocumentKey = documentKey, NullableLanguageId = null, Alias = "alias-one" },
            new PublishedDocumentUrlAlias { DocumentKey = documentKey, NullableLanguageId = null, Alias = "alias-two" },
        };
        var languages = new List<ILanguage> { CreateMockLanguage(1, "en-US") };

        var service = await CreateInitializedServiceWithAliases(seeded, languages);

        // Act
        var result = (await service.GetAliasesAsync(documentKey, culture: null)).ToList();

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { "alias-one", "alias-two" }));
    }

    /// <summary>
    /// After initializing from a non-empty set of aliases, <see cref="DocumentUrlAliasService.HasAny"/>
    /// should report that aliases exist. Initializing from an empty set must report the opposite.
    /// </summary>
    [Test]
    public async Task HasAny_ReflectsWhetherAnyAliasesWereInitialized()
    {
        // Arrange
        var languages = new List<ILanguage> { CreateMockLanguage(1, "en-US") };
        var populatedService = await CreateInitializedServiceWithAliases(
            new[]
            {
                new PublishedDocumentUrlAlias
                {
                    DocumentKey = Guid.NewGuid(),
                    NullableLanguageId = null,
                    Alias = "any",
                },
            },
            languages);
        var emptyService = await CreateInitializedServiceWithAliases(
            Array.Empty<PublishedDocumentUrlAlias>(),
            languages);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(populatedService.HasAny(), Is.True);
            Assert.That(emptyService.HasAny(), Is.False);
        });
    }

    #endregion
}
