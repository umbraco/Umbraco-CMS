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

/// <summary>
/// <see cref="DocumentUrlAliasService.PersistAliasesAsync"/> runs inside the transaction that persists the document,
/// so these tests pin what it may and may not do there: write only when the published or trashed state changed,
/// never load the document, and never take a distributed lock while an ambient scope already holds one.
/// </summary>
[TestFixture]
public class DocumentUrlAliasServicePersistAliasesTests
{
    private sealed record ServiceUnderTest(
        DocumentUrlAliasService Service,
        Mock<IDocumentUrlAliasRepository> AliasRepository,
        Mock<IContentService> ContentService,
        Mock<ICoreScope> Scope);

    private static ServiceUnderTest CreateService(bool hasAmbientScope = true, IEnumerable<ILanguage>? languages = null)
    {
        var aliasRepositoryMock = new Mock<IDocumentUrlAliasRepository>();
        var contentServiceMock = new Mock<IContentService>(MockBehavior.Strict);

        var languageServiceMock = new Mock<ILanguageService>();
        languageServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(languages ?? []);
        languageServiceMock.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("en-US");

        var keyValueServiceMock = new Mock<IKeyValueService>();
        keyValueServiceMock.Setup(x => x.GetValue(DocumentUrlAliasService.RebuildKey)).Returns("1");

        var serverRoleAccessorMock = new Mock<IServerRoleAccessor>();
        serverRoleAccessorMock.Setup(x => x.CurrentServerRole).Returns(ServerRole.Single);

        var scopeContextMock = new Mock<IScopeContext>();
        var coreScopeMock = new Mock<ICoreScope>();

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
        coreScopeProviderMock.Setup(x => x.Context).Returns(hasAmbientScope ? scopeContextMock.Object : null);

        var service = new DocumentUrlAliasService(
            Mock.Of<ILogger<DocumentUrlAliasService>>(),
            aliasRepositoryMock.Object,
            coreScopeProviderMock.Object,
            languageServiceMock.Object,
            keyValueServiceMock.Object,
            contentServiceMock.Object,
            Mock.Of<IDocumentNavigationQueryService>(),
            serverRoleAccessorMock.Object);

        return new ServiceUnderTest(service, aliasRepositoryMock, contentServiceMock, coreScopeMock);
    }

    private static Mock<IContent> CreateInvariantContent(
        string? aliasValue,
        PublishedState publishedState = PublishedState.Published,
        bool trashed = false,
        bool trashedChanged = false,
        bool blueprint = false)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Variations).Returns(ContentVariation.Nothing);

        var propertyCollectionMock = new Mock<IPropertyCollection>();
        propertyCollectionMock.Setup(x => x.GetEnumerator())
            .Returns(() => Enumerable.Empty<IProperty>().GetEnumerator());

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(Guid.NewGuid());
        contentMock.Setup(x => x.Trashed).Returns(trashed);
        contentMock.Setup(x => x.Blueprint).Returns(blueprint);
        contentMock.Setup(x => x.PublishedState).Returns(publishedState);
        contentMock.Setup(x => x.IsPropertyDirty(nameof(IContent.Trashed))).Returns(trashedChanged);
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentMock.Setup(x => x.Properties).Returns(propertyCollectionMock.Object);
        contentMock.Setup(x => x.GetValue<string>(
                Constants.Conventions.Content.UrlAlias,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Returns(aliasValue);

        return contentMock;
    }

    private static Mock<IContent> CreateVariantContent(Dictionary<string, string?> aliasesByCulture, PublishedState publishedState)
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
            .Returns(() => new List<IProperty> { aliasPropertyMock.Object }.GetEnumerator());

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(Guid.NewGuid());
        contentMock.Setup(x => x.PublishedState).Returns(publishedState);
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentMock.Setup(x => x.Properties).Returns(propertyCollectionMock.Object);
        contentMock.Setup(x => x.GetValue<string>(
                Constants.Conventions.Content.UrlAlias,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Returns((string _, string? culture, string? _, bool _) =>
                culture is not null && aliasesByCulture.TryGetValue(culture, out var alias) ? alias : null);

        return contentMock;
    }

    private static ILanguage CreateLanguage(int id, string isoCode)
    {
        var languageMock = new Mock<ILanguage>();
        languageMock.Setup(x => x.Id).Returns(id);
        languageMock.Setup(x => x.IsoCode).Returns(isoCode);
        return languageMock.Object;
    }

    [TestCase(PublishedState.Published)]
    [TestCase(PublishedState.Unpublished)]
    public async Task PersistAliasesAsync_SaveThatCannotChangeAliases_DoesNotTouchTheRepository(PublishedState publishedState)
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent("my-alias", publishedState);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.AliasRepository.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()), Times.Never);
        sut.AliasRepository.Verify(x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_Publishing_WithAlias_SavesTheNormalizedAliases()
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent(" /First-Alias/ , second-alias, first-alias", PublishedState.Publishing);
        List<PublishedDocumentUrlAlias>? saved = null;
        sut.AliasRepository.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()))
            .Callback<IEnumerable<PublishedDocumentUrlAlias>>(aliases => saved = aliases.ToList());

        await sut.Service.PersistAliasesAsync(content.Object);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Select(x => x.Alias), Is.EqualTo(new[] { "first-alias", "second-alias" }));
        Assert.That(saved.All(x => x.DocumentKey == content.Object.Key && x.NullableLanguageId is null), Is.True);
        sut.AliasRepository.Verify(x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_Publishing_WithoutAlias_DeletesTheDocumentsRows()
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent(null, PublishedState.Publishing);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.AliasRepository.Verify(x => x.DeleteByDocumentKey(It.Is<IEnumerable<Guid>>(keys => keys.Single() == content.Object.Key)), Times.Once);
        sut.AliasRepository.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_Unpublishing_DeletesTheDocumentsRows_WhateverTheAliasValueIs()
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent("still-set-on-the-entity", PublishedState.Unpublishing);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.AliasRepository.Verify(x => x.DeleteByDocumentKey(It.Is<IEnumerable<Guid>>(keys => keys.Single() == content.Object.Key)), Times.Once);
        sut.AliasRepository.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_TrashedDocument_DeletesTheDocumentsRows()
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Published, trashed: true, trashedChanged: true);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.AliasRepository.Verify(x => x.DeleteByDocumentKey(It.Is<IEnumerable<Guid>>(keys => keys.Single() == content.Object.Key)), Times.Once);
        sut.AliasRepository.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_RestoredDocument_SavesItsPublishedAliasesAgain()
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Published, trashed: false, trashedChanged: true);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.AliasRepository.Verify(
            x => x.Save(It.Is<IEnumerable<PublishedDocumentUrlAlias>>(aliases => aliases.Single().Alias == "my-alias")),
            Times.Once);
    }

    [Test]
    public async Task PersistAliasesAsync_Blueprint_DoesNotTouchTheRepository()
    {
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Publishing, blueprint: true);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.AliasRepository.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()), Times.Never);
        sut.AliasRepository.Verify(x => x.DeleteByDocumentKey(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_VariantAliasProperty_SavesOneRowPerPublishedCulture()
    {
        var languages = new[] { CreateLanguage(1, "en-US"), CreateLanguage(2, "fr-FR"), CreateLanguage(3, "da-DK") };
        ServiceUnderTest sut = CreateService(languages: languages);
        Mock<IContent> content = CreateVariantContent(
            new Dictionary<string, string?> { ["en-US"] = "english", ["fr-FR"] = "francais", ["da-DK"] = null },
            PublishedState.Publishing);
        List<PublishedDocumentUrlAlias>? saved = null;
        sut.AliasRepository.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlAlias>>()))
            .Callback<IEnumerable<PublishedDocumentUrlAlias>>(aliases => saved = aliases.ToList());

        await sut.Service.PersistAliasesAsync(content.Object);

        Assert.That(saved, Is.Not.Null);
        Assert.That(
            saved!.Select(x => (x.NullableLanguageId, x.Alias)),
            Is.EquivalentTo(new[] { ((int?)1, "english"), ((int?)2, "francais") }));
    }

    [Test]
    public async Task PersistAliasesAsync_NeverLoadsTheDocument()
    {
        // The content service mock is strict, so any GetById call would throw; the explicit verifies make the
        // intent visible when the test is read.
        ServiceUnderTest sut = CreateService();
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Publishing);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.ContentService.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never);
        sut.ContentService.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task PersistAliasesAsync_WhenTheCallerHoldsTheContentTreeWriteLock_TakesNoLockOfItsOwn()
    {
        ServiceUnderTest sut = CreateService(hasAmbientScope: true);
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Publishing);

        await sut.Service.PersistAliasesAsync(content.Object, contentTreeWriteLockHeld: true);

        sut.Scope.Verify(x => x.WriteLock(It.IsAny<int[]>()), Times.Never);
        sut.Scope.Verify(x => x.WriteLock(It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Never);
        sut.Scope.Verify(x => x.ReadLock(It.IsAny<int[]>()), Times.Never);
        sut.Scope.Verify(x => x.Complete(), Times.Once);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task PersistAliasesAsync_WhenTheCallerDoesNotHoldTheContentTreeWriteLock_TakesContentTreeRead_ThenAliasWrite(bool hasAmbientScope)
    {
        // An ambient scope says nothing about the locks it holds, so the locks are taken either way.
        ServiceUnderTest sut = CreateService(hasAmbientScope);
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Publishing);
        var locks = new List<(string Type, int LockId)>();
        sut.Scope.Setup(x => x.ReadLock(It.IsAny<int[]>()))
            .Callback<int[]>(lockIds => locks.AddRange(lockIds.Select(id => ("read", id))));
        sut.Scope.Setup(x => x.WriteLock(It.IsAny<int[]>()))
            .Callback<int[]>(lockIds => locks.AddRange(lockIds.Select(id => ("write", id))));

        await sut.Service.PersistAliasesAsync(content.Object, contentTreeWriteLockHeld: false);

        // Shared on the content tree so the write cannot overlap a save, exclusive on the alias table so it cannot
        // overlap the rebuild, in the order the rebuild uses.
        Assert.That(
            locks,
            Is.EqualTo(new[] { ("read", Constants.Locks.ContentTree), ("write", Constants.Locks.DocumentUrlAliases) }));
        sut.Scope.Verify(x => x.Complete(), Times.Once);
    }

    [Test]
    public async Task PersistAliasesAsync_WithoutTheLockArgument_DoesNotAssumeTheContentTreeWriteLock()
    {
        ServiceUnderTest sut = CreateService(hasAmbientScope: true);
        Mock<IContent> content = CreateInvariantContent("my-alias", PublishedState.Publishing);

        await sut.Service.PersistAliasesAsync(content.Object);

        sut.Scope.Verify(x => x.ReadLock(It.Is<int[]>(ids => ids.Single() == Constants.Locks.ContentTree)), Times.Once);
        sut.Scope.Verify(x => x.WriteLock(It.Is<int[]>(ids => ids.Single() == Constants.Locks.DocumentUrlAliases)), Times.Once);
    }

    [Test]
    public void IsInitialized_IsFalseUntilInitAsyncCompletes()
    {
        ServiceUnderTest sut = CreateService();

        Assert.That(sut.Service.IsInitialized, Is.False);
        sut.Service.InitAsync(forceEmpty: true, CancellationToken.None).GetAwaiter().GetResult();
        Assert.That(sut.Service.IsInitialized, Is.True);
    }

    [Test]
    public async Task InterfaceDefaults_KeepAnExistingImplementationOnItsOldPath()
    {
        var implementation = new MinimalAliasService();
        IContent content = CreateInvariantContent("my-alias", PublishedState.Publishing).Object;

        await ((IDocumentUrlAliasService)implementation).PersistAliasesAsync(content);
        await ((IDocumentUrlAliasService)implementation).PersistAliasesAsync(content, contentTreeWriteLockHeld: true);

        Assert.That(((IDocumentUrlAliasService)implementation).IsInitialized, Is.True);
        Assert.That(implementation.CreatedOrUpdatedKeys, Is.EqualTo(new[] { content.Key, content.Key }));
    }

    /// <summary>
    /// Implements only the members that existed before <see cref="IDocumentUrlAliasService.PersistAliasesAsync"/> was
    /// added, as an external implementation compiled against the previous interface would.
    /// </summary>
    private sealed class MinimalAliasService : IDocumentUrlAliasService
    {
        public List<Guid> CreatedOrUpdatedKeys { get; } = [];

        public Task InitAsync(bool forceEmpty, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IEnumerable<Guid>> GetDocumentKeysByAliasAsync(string alias, string? culture) => Task.FromResult(Enumerable.Empty<Guid>());

        public Task<IEnumerable<string>> GetAliasesAsync(Guid documentKey, string? culture) => Task.FromResult(Enumerable.Empty<string>());

        public Task CreateOrUpdateAliasesAsync(Guid documentKey)
        {
            CreatedOrUpdatedKeys.Add(documentKey);
            return Task.CompletedTask;
        }

        public Task CreateOrUpdateAliasesWithDescendantsAsync(Guid documentKey) => Task.CompletedTask;

        public Task DeleteAliasesFromCacheAsync(IEnumerable<Guid> documentKeys) => Task.CompletedTask;

        public Task RebuildAllAliasesAsync() => Task.CompletedTask;

        public bool HasAny() => false;
    }
}
