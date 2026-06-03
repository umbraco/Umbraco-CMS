// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AsyncDocumentRepositoryTest : UmbracoIntegrationTest
{
    private ContentType _contentType = null!;
    private Content _textpage = null!;
    private Content _subpage = null!;
    private Content _subpage2 = null!;
    private Content _trashed = null!;
    private Content _publishedPage = null!;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    [SetUp]
    public async Task SetUpData()
    {
        await CreateTestData();
        ContentRepositoryBase.ThrowOnWarning = true;
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private async Task CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        _contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        _contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

        _textpage = ContentBuilder.CreateSimpleContent(_contentType);
        _textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
        ContentService.Save(_textpage, -1);

        _subpage = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 1", _textpage.Id);
        _subpage.Key = new Guid("FF11402B-7E53-4654-81A7-462AC2108059");
        ContentService.Save(_subpage, -1);

        _subpage2 = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 2", _textpage.Id);
        ContentService.Save(_subpage2, -1);

        _trashed = ContentBuilder.CreateSimpleContent(_contentType, "Text Page Deleted", -20);
        _trashed.Trashed = true;
        ContentService.Save(_trashed, -1);

        _publishedPage = ContentBuilder.CreateSimpleContent(_contentType, "Published Page");
        ContentService.Save(_publishedPage, -1);
        ContentService.Publish(_publishedPage, ["*"]);
    }

    private AsyncDocumentRepository CreateRepository() => new(
        GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
        AppCaches.Disabled,
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
        GetRequiredService<IContentTypeRepository>());

    // --- PerformGetAsync ---

    [Test]
    public async Task GetAsync_WithExistingKey_ReturnsSingleDocument()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_textpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.HasIdentity, Is.True);
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_WithNonExistentKey_ReturnsNull()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_PopulatesNodeMetadata()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_subpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Key, Is.EqualTo(_subpage.Key));
            Assert.That(result.Name, Is.EqualTo(_subpage.Name));
            Assert.That(result.ParentId, Is.EqualTo(_textpage.Id));
            Assert.That(result.Level, Is.EqualTo(2));
            Assert.That(result.SortOrder, Is.EqualTo(_subpage.SortOrder));
            Assert.That(result.CreateDate, Is.EqualTo(_subpage.CreateDate).Within(TimeSpan.FromSeconds(1)));
        });
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_PopulatesContentType()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_textpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ContentType, Is.Not.Null);
        Assert.That(result.ContentType.Alias, Is.EqualTo(_contentType.Alias));
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_PopulatesProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_textpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Properties, Is.Not.Empty);
        Assert.That(result.GetValue("title"), Is.EqualTo("Welcome to our Home page"));
        Assert.That(result.GetValue("bodyText"), Is.EqualTo("This is the welcome message on the first page"));
        Assert.That(result.GetValue("author"), Is.EqualTo("John Doe"));
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_UnpublishedDocument_HasPublishedFalse()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_textpage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Published, Is.False);
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_PublishedDocument_HasPublishedTrue()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_publishedPage.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Published, Is.True);
        scope.Complete();
    }

    [Test]
    public async Task GetAsync_TrashedDocument_ReturnsTrashedEntity()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetAsync(_trashed.Key, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Trashed, Is.True);
        scope.Complete();
    }

    // --- PerformGetAllAsync ---

    [Test]
    public async Task GetAllAsync_ReturnsAllDocuments()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetAllAsync(CancellationToken.None);

        // textpage, subpage, subpage2, trashed, publishedPage
        Assert.That(results.Count(), Is.EqualTo(5));
        scope.Complete();
    }

    [Test]
    public async Task GetAllAsync_IncludesTrashedDocuments()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetAllAsync(CancellationToken.None);

        Assert.That(results.Any(c => c.Key == _trashed.Key), Is.True);
        scope.Complete();
    }

    [Test]
    public async Task GetAllAsync_EachDocumentHasContentTypePopulated()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetAllAsync(CancellationToken.None);

        Assert.That(results.All(c => c.ContentType != null), Is.True);
        scope.Complete();
    }

    // --- PerformGetManyAsync ---

    [Test]
    public async Task GetManyAsync_WithSubsetOfKeys_ReturnsOnlyRequestedDocuments()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetManyAsync(
            [_textpage.Key, _subpage.Key],
            CancellationToken.None);

        Assert.That(results.Count(), Is.EqualTo(2));
        Assert.That(results.Any(c => c.Key == _textpage.Key), Is.True);
        Assert.That(results.Any(c => c.Key == _subpage.Key), Is.True);
        scope.Complete();
    }

    [Test]
    public async Task GetManyAsync_WithNonExistentKeysMixed_ReturnsOnlyExisting()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetManyAsync(
            [_textpage.Key, Guid.NewGuid()],
            CancellationToken.None);

        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.Single().Key, Is.EqualTo(_textpage.Key));
        scope.Complete();
    }

    [Test]
    public async Task GetManyAsync_WithEmptyArray_ReturnsAllDocuments()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetManyAsync([], CancellationToken.None);

        Assert.That(results.Count(), Is.EqualTo(5));
        scope.Complete();
    }

    [Test]
    public async Task GetManyAsync_DeduplicatesKeys()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetManyAsync(
            [_textpage.Key, _textpage.Key],
            CancellationToken.None);

        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.Single().Key, Is.EqualTo(_textpage.Key));
        scope.Complete();
    }

    // --- Parity with NPoco path ---

    [Test]
    public async Task GetAsync_MatchesContentServiceOnScalarFields()
    {
        IContent? fromService = ContentService.GetById(_textpage.Key);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? fromRepository = await repository.GetAsync(_textpage.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(fromService, Is.Not.Null);
        Assert.That(fromRepository, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fromRepository!.Key, Is.EqualTo(fromService!.Key));
            Assert.That(fromRepository.Name, Is.EqualTo(fromService.Name));
            Assert.That(fromRepository.Path, Is.EqualTo(fromService.Path));
            Assert.That(fromRepository.Level, Is.EqualTo(fromService.Level));
            Assert.That(fromRepository.ParentId, Is.EqualTo(fromService.ParentId));
            Assert.That(fromRepository.Published, Is.EqualTo(fromService.Published));
            Assert.That(fromRepository.ContentType.Alias, Is.EqualTo(fromService.ContentType.Alias));
        });
    }

    [Test]
    public async Task GetAsync_MatchesContentServiceOnProperties()
    {
        IContent? fromService = ContentService.GetById(_textpage.Key);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? fromRepository = await repository.GetAsync(_textpage.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(fromService, Is.Not.Null);
        Assert.That(fromRepository, Is.Not.Null);
        foreach (IProperty property in fromService!.Properties)
        {
            Assert.That(
                fromRepository!.GetValue(property.Alias),
                Is.EqualTo(fromService.GetValue(property.Alias)),
                $"Property '{property.Alias}' differs between EF Core and NPoco paths");
        }
    }
}
