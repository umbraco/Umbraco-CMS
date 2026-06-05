// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.EntityFrameworkCore;
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
using Umbraco.Cms.Infrastructure.Persistence;
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
    private ITemplate _template = null!;
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
        _template = template;

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

    // --- Published/draft property split ---

    [Test]
    public async Task GetAsync_EditedAfterPublish_HasBothDraftAndPublishedPropertyValues()
    {
        // _publishedPage was saved with title="Welcome to our Home page" and then published in SetUp.
        // Edit the title in the draft without re-publishing so the two versions diverge.
        _publishedPage.SetValue("title", "draft edit");
        ContentService.Save(_publishedPage, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(_publishedPage.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetValue("title"), Is.EqualTo("draft edit"),
            "draft value should reflect the unsaved edit");
        Assert.That(result.GetValue("title", published: true), Is.EqualTo("Welcome to our Home page"),
            "published value should remain unchanged");
    }

    // --- Culture variations (ApplyVariationsAsync) ---

    // Helper: creates a French language and a culture-variant content type with a variant text property.
    // en-US is already installed as the default language by the migration runner.
    private async Task<IContentType> CreateVariantContentTypeAsync()
    {
        await GetRequiredService<ILanguageService>().CreateAsync(
            new Language("fr", "French"),
            Constants.Security.SuperUserKey);

        var propertyCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "variantTitle", ValueStorageType.Ntext)
            {
                Alias = "variantTitle",
                DataTypeId = -88,
                Variations = ContentVariation.Culture,
            },
        };

        var contentType = ContentTypeBuilder.CreateBasicContentType("umbVariant", "Variant");
        contentType.Variations = ContentVariation.Culture;
        contentType.PropertyGroups.Add(new PropertyGroup(propertyCollection) { Alias = "content", Name = "Content" });
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    [Test]
    public async Task GetAsync_VariantDocument_HasDraftCultureNames()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        IContent doc = ContentBuilder.CreateBasicContent(contentType);
        doc.SetCultureName("English Name", "en-US");
        doc.SetCultureName("Nom Français", "fr");
        ContentService.Save(doc);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(doc.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetCultureName("en-US"), Is.EqualTo("English Name"));
        Assert.That(result.GetCultureName("fr"), Is.EqualTo("Nom Français"));
    }

    [Test]
    public async Task GetAsync_PublishedVariantDocument_HasPublishedCultureNames()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        IContent doc = ContentBuilder.CreateBasicContent(contentType);
        doc.SetCultureName("English Name", "en-US");
        doc.SetCultureName("Nom Français", "fr");
        ContentService.Save(doc);
        ContentService.Publish(doc, doc.AvailableCultures.ToArray());

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(doc.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetPublishName("en-US"), Is.EqualTo("English Name"));
        Assert.That(result.GetPublishName("fr"), Is.EqualTo("Nom Français"));
    }

    [Test]
    public async Task GetAsync_VariantDocument_EditedCultureIsMarked()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        IContent doc = ContentBuilder.CreateBasicContent(contentType);
        doc.SetCultureName("English Name", "en-US");
        doc.SetCultureName("Nom Français", "fr");
        ContentService.Save(doc);
        ContentService.Publish(doc, doc.AvailableCultures.ToArray());

        // Re-fetch so the entity has the published state, then edit fr only.
        doc = ContentService.GetById(doc.Key)!;
        doc.SetCultureName("Nom Modifié", "fr");
        ContentService.Save(doc);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(doc.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCultureEdited("fr"), Is.True,
            "fr was edited after publishing so it should be marked edited");
        Assert.That(result.IsCultureEdited("en-US"), Is.False,
            "en-US was not touched after publishing so it should not be marked edited");
    }

    [Test]
    public async Task GetAsync_VariantDocument_HasCultureSpecificPropertyValues()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        IContent doc = ContentBuilder.CreateBasicContent(contentType);
        doc.SetCultureName("English Name", "en-US");
        doc.SetCultureName("Nom Français", "fr");
        doc.SetValue("variantTitle", "English Title", "en-US");
        doc.SetValue("variantTitle", "Titre Français", "fr");
        ContentService.Save(doc);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(doc.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetValue("variantTitle", "en-US"), Is.EqualTo("English Title"));
        Assert.That(result.GetValue("variantTitle", "fr"), Is.EqualTo("Titre Français"));
    }

    // --- Template IDs ---

    [Test]
    public async Task GetAsync_PublishedDocumentWithTemplate_PopulatesTemplateIds()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Templated Page");
        content.TemplateId = _template.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["*"]);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TemplateId, Is.EqualTo(_template.Id),
            "TemplateId should reflect the current draft's template");
        // PublishTemplateId comes from the published DocumentVersion join. The NPoco
        // single-entity path (AddAdditionalContentMapping) skips this field; the NPoco
        // bulk path and the EF Core path both set it from the published version's TemplateId.
        Assert.That(result.PublishTemplateId, Is.EqualTo(_template.Id),
            "PublishTemplateId should reflect the template that was active when the document was published");
    }

    // --- Partial culture publication ---

    [Test]
    public async Task GetAsync_PartiallyPublishedVariantDocument_OnlyPublishedCultureHasPublishInfo()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        IContent doc = ContentBuilder.CreateBasicContent(contentType);
        doc.SetCultureName("English Name", "en-US");
        doc.SetCultureName("Nom Français", "fr");
        ContentService.Save(doc);
        ContentService.Publish(doc, ["en-US"]); // publish only en-US, leave fr as draft

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(doc.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCulturePublished("en-US"), Is.True,
            "en-US was published so it should have publish info");
        Assert.That(result.IsCulturePublished("fr"), Is.False,
            "fr was never published so it should not have publish info");
        Assert.That(result.GetPublishName("en-US"), Is.EqualTo("English Name"));
        Assert.That(result.GetPublishName("fr"), Is.Null);
    }

    // --- Group 8: GetVersionAsync ---

    [Test]
    public async Task GetVersionAsync_WithValidVersionKey_ReturnsVersion()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Guid versionKey = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions
                .Where(contentVersion => contentVersion.NodeId == _publishedPage.Id && contentVersion.Current)
                .Select(contentVersion => contentVersion.Key)
                .FirstOrDefaultAsync());

        IContent? result = await repository.GetVersionAsync(versionKey, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(_publishedPage.Key));
        Assert.That(result.Name, Is.EqualTo(_publishedPage.Name));
    }

    [Test]
    public async Task GetVersionAsync_WithNonExistentVersionKey_ReturnsNull()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IContent? result = await repository.GetVersionAsync(Guid.NewGuid(), CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetVersionAsync_PopulatesProperties()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Guid versionKey = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions
                .Where(contentVersion => contentVersion.NodeId == _publishedPage.Id && contentVersion.Current)
                .Select(contentVersion => contentVersion.Key)
                .FirstOrDefaultAsync());

        IContent? result = await repository.GetVersionAsync(versionKey, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Properties, Is.Not.Empty);
    }

    // --- Group 9: GetAllVersionsAsync ---

    [Test]
    public async Task GetAllVersionsAsync_WithMultipleVersions_ReturnsAllInOrder()
    {
        // _publishedPage was created via SaveAndPublish which inserts two ContentVersion rows:
        // one pre-publish draft and one post-publish current version. That guarantees >= 2 versions.
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetAllVersionsAsync(_publishedPage.Key, CancellationToken.None);
        scope.Complete();

        IContent[] versions = results.ToArray();
        Assert.That(versions, Has.Length.GreaterThanOrEqualTo(2));
        // Current version (Current = true) is ordered first, and it is the published one.
        Assert.That(versions[0].Published, Is.True, "first result should be the current published version");
    }

    [Test]
    public async Task GetAllVersionsAsync_WithNonExistentNodeKey_ReturnsEmpty()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetAllVersionsAsync(Guid.NewGuid(), CancellationToken.None);
        scope.Complete();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task GetAllVersionsAsync_EachVersionHasProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> results = await repository.GetAllVersionsAsync(_publishedPage.Key, CancellationToken.None);
        scope.Complete();

        foreach (IContent version in results)
        {
            Assert.That(version.Properties, Is.Not.Empty,
                $"Version with VersionId {version.VersionId} should have properties");
        }
    }

    // --- Group 10: EF Core / NPoco parity for version methods (temporary) ---
    // These tests exist to build confidence that the EF Core version methods
    // return the same data as the NPoco equivalents. Remove once parity is established.

    [Test]
    public async Task GetVersionAsync_MatchesNPocoOnScalarFields()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Guid versionKey = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions
                .Where(contentVersion => contentVersion.NodeId == _publishedPage.Id && contentVersion.Current)
                .Select(contentVersion => contentVersion.Key)
                .FirstOrDefaultAsync());

        IContent? fromEfCore = await repository.GetVersionAsync(versionKey, CancellationToken.None);
        scope.Complete();

        Assert.That(fromEfCore, Is.Not.Null);
        IContent? fromNPoco = ContentService.GetVersion(fromEfCore!.VersionId);
        Assert.That(fromNPoco, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(fromEfCore.Key, Is.EqualTo(fromNPoco!.Key));
            Assert.That(fromEfCore.Name, Is.EqualTo(fromNPoco.Name));
            Assert.That(fromEfCore.Path, Is.EqualTo(fromNPoco.Path));
            Assert.That(fromEfCore.Level, Is.EqualTo(fromNPoco.Level));
            Assert.That(fromEfCore.ParentId, Is.EqualTo(fromNPoco.ParentId));
            Assert.That(fromEfCore.Published, Is.EqualTo(fromNPoco.Published));
            Assert.That(fromEfCore.ContentType.Alias, Is.EqualTo(fromNPoco.ContentType.Alias));
            Assert.That(fromEfCore.VersionId, Is.EqualTo(fromNPoco.VersionId));
        });
    }

    [Test]
    public async Task GetVersionAsync_MatchesNPocoOnProperties()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        Guid versionKey = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions
                .Where(contentVersion => contentVersion.NodeId == _publishedPage.Id && contentVersion.Current)
                .Select(contentVersion => contentVersion.Key)
                .FirstOrDefaultAsync());

        IContent? fromEfCore = await repository.GetVersionAsync(versionKey, CancellationToken.None);
        scope.Complete();

        Assert.That(fromEfCore, Is.Not.Null);
        IContent? fromNPoco = ContentService.GetVersion(fromEfCore!.VersionId);
        Assert.That(fromNPoco, Is.Not.Null);

        foreach (IProperty property in fromNPoco!.Properties)
        {
            Assert.That(
                fromEfCore.GetValue(property.Alias),
                Is.EqualTo(fromNPoco.GetValue(property.Alias)),
                $"Property '{property.Alias}' differs between EF Core and NPoco for GetVersionAsync");
        }
    }

    [Test]
    public async Task GetAllVersionsAsync_MatchesNPocoOnCount()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> fromEfCore = await repository.GetAllVersionsAsync(_publishedPage.Key, CancellationToken.None);
        scope.Complete();

        IContent[] efCoreVersions = fromEfCore.ToArray();
        IContent[] npocoVersions = ContentService.GetVersions(_publishedPage.Id).ToArray();

        Assert.That(efCoreVersions, Has.Length.EqualTo(npocoVersions.Length));
    }

    [Test]
    public async Task GetAllVersionsAsync_MatchesNPocoOnScalarFieldsPerVersion()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> fromEfCore = await repository.GetAllVersionsAsync(_publishedPage.Key, CancellationToken.None);
        scope.Complete();

        Dictionary<int, IContent> efCoreByVersionId = fromEfCore.ToDictionary(version => version.VersionId);
        Dictionary<int, IContent> npocoByVersionId = ContentService.GetVersions(_publishedPage.Id)
            .ToDictionary(version => version.VersionId);

        Assert.That(efCoreByVersionId.Keys, Is.EquivalentTo(npocoByVersionId.Keys),
            "EF Core and NPoco should return the same set of version IDs");

        foreach (KeyValuePair<int, IContent> efCorePair in efCoreByVersionId)
        {
            IContent efCoreVersion = efCorePair.Value;
            IContent npocoVersion = npocoByVersionId[efCorePair.Key];

            Assert.Multiple(() =>
            {
                Assert.That(efCoreVersion.Key, Is.EqualTo(npocoVersion.Key),
                    $"Key differs for VersionId {efCorePair.Key}");
                Assert.That(efCoreVersion.Name, Is.EqualTo(npocoVersion.Name),
                    $"Name differs for VersionId {efCorePair.Key}");
                Assert.That(efCoreVersion.Published, Is.EqualTo(npocoVersion.Published),
                    $"Published differs for VersionId {efCorePair.Key}");
                Assert.That(efCoreVersion.ContentType.Alias, Is.EqualTo(npocoVersion.ContentType.Alias),
                    $"ContentType.Alias differs for VersionId {efCorePair.Key}");
            });
        }
    }

    // --- Group 11: GetChildrenAsync ---

    [Test]
    public async Task GetChildrenAsync_WithChildren_ReturnsDirectChildrenOnly()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(2));
        Assert.That(result.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Items.Any(c => c.Key == _subpage.Key), Is.True);
        Assert.That(result.Items.Any(c => c.Key == _subpage2.Key), Is.True);
        Assert.That(result.Items.All(c => c.ParentId == _textpage.Id), Is.True,
            "GetChildrenAsync should return only direct children, not grandchildren");
    }

    [Test]
    public async Task GetChildrenAsync_WithPaging_ReturnsCorrectPage()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 1, take: 1, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(2), "Total should be 2 regardless of paging");
        Assert.That(result.Items.Count(), Is.EqualTo(1), "take=1 should return exactly 1 item");
    }

    [Test]
    public async Task GetChildrenAsync_WithNonExistentParentKey_ReturnsEmpty()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            Guid.NewGuid(), skip: 0, take: 100, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(0));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetChildrenAsync_DefaultOrdering_ReturnsBySortOrder()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        IContent[] children = result.Items.ToArray();
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.That(children[0].SortOrder, Is.LessThanOrEqualTo(children[1].SortOrder),
            "Children should be ordered by SortOrder ascending by default");
    }

    [Test]
    public async Task GetChildrenAsync_PropertyAliasNull_LoadsAllProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 1, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        IContent child = result.Items.First();
        Assert.That(child.Properties, Is.Not.Empty, "null propertyAliases should load all properties");
    }

    [Test]
    public async Task GetChildrenAsync_PropertyAliasEmpty_LoadsNoProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 1, propertyAliases: [], ordering: null, CancellationToken.None);
        scope.Complete();

        IContent child = result.Items.First();
        Assert.That(child.Properties.Where(p => p.GetValue() != null), Is.Empty,
            "empty propertyAliases should load no property data");
    }

    // --- Group 12: GetDescendantsAsync ---

    [Test]
    public async Task GetDescendantsAsync_WithDescendants_ReturnsAllDescendants()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            _textpage.Key, skip: 0, take: 100, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(2));
        Assert.That(result.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Items.Any(c => c.Key == _subpage.Key), Is.True);
        Assert.That(result.Items.Any(c => c.Key == _subpage2.Key), Is.True);
    }

    [Test]
    public async Task GetDescendantsAsync_WithPaging_ReturnsCorrectPage()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            _textpage.Key, skip: 0, take: 1, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(2), "Total should be 2 regardless of paging");
        Assert.That(result.Items.Count(), Is.EqualTo(1), "take=1 should return exactly 1 item");
    }

    [Test]
    public async Task GetDescendantsAsync_WithNonExistentAncestorKey_ReturnsEmpty()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            Guid.NewGuid(), skip: 0, take: 100, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(0));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetDescendantsAsync_EachDescendantHasProperties()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            _textpage.Key, skip: 0, take: 100, ordering: null, CancellationToken.None);
        scope.Complete();

        foreach (IContent descendant in result.Items)
        {
            Assert.That(descendant.Properties, Is.Not.Empty,
                $"Descendant {descendant.Key} should have properties populated");
        }
    }

    // --- Variant published/draft property split ---

    [Test]
    public async Task GetAsync_VariantDocument_EditedAfterPublish_HasBothDraftAndPublishedPropertyValues()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        IContent doc = ContentBuilder.CreateBasicContent(contentType);
        doc.SetCultureName("English Name", "en-US");
        doc.SetCultureName("Nom Français", "fr");
        doc.SetValue("variantTitle", "published value", "en-US");
        ContentService.Save(doc);
        ContentService.Publish(doc, ["en-US", "fr"]);

        // Edit the draft value without re-publishing.
        doc = ContentService.GetById(doc.Key)!;
        doc.SetValue("variantTitle", "draft value", "en-US");
        ContentService.Save(doc);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        IContent? result = await repository.GetAsync(doc.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetValue("variantTitle", "en-US"), Is.EqualTo("draft value"),
            "draft property value should reflect the unsaved edit");
        Assert.That(result.GetValue("variantTitle", "en-US", published: true), Is.EqualTo("published value"),
            "published property value should remain unchanged");
    }
}
