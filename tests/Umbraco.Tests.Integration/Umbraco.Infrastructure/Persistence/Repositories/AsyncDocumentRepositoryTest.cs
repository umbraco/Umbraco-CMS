using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
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

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

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
        GetRequiredService<IContentTypeRepository>(),
        GetRequiredService<ITemplateRepository>(),
        GetRequiredService<IIdKeyMap>(),
        GetRequiredService<ITagRepository>(),
        GetRequiredService<IJsonSerializer>());

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

    // EF Core / NPoco parity for version methods (temporary) ---
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

    [Test]
    public async Task GetChildrenWithoutTemplatesAsync_ReturnsItemsWithNullTemplateIds()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Templated Child", _textpage.Id);
        content.TemplateId = _template.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["*"]);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenWithoutTemplatesAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Items.Any(c => c.Key == content.Key), Is.True);
        Assert.That(result.Items.All(c => c.TemplateId == null), Is.True,
            "GetChildrenWithoutTemplatesAsync must not populate TemplateId");
        Assert.That(result.Items.All(c => c.PublishTemplateId == null), Is.True,
            "GetChildrenWithoutTemplatesAsync must not populate PublishTemplateId");
    }

    [Test]
    public async Task GetChildrenAsync_WithTemplate_PopulatesTemplateId()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Templated Child For Verify", _textpage.Id);
        content.TemplateId = _template.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["*"]);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null, ordering: null, CancellationToken.None);
        scope.Complete();

        IContent? templated = result.Items.FirstOrDefault(c => c.Key == content.Key);
        Assert.That(templated, Is.Not.Null);
        Assert.That(templated!.TemplateId, Is.EqualTo(_template.Id),
            "GetChildrenAsync must populate TemplateId for content with a template assigned");
    }

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

    [Test]
    public async Task GetDescendantsWithoutTemplatesAsync_ReturnsItemsWithNullTemplateIds()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Templated Descendant", _textpage.Id);
        content.TemplateId = _template.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["*"]);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsWithoutTemplatesAsync(
            _textpage.Key, skip: 0, take: 100, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(result.Items.Any(c => c.Key == content.Key), Is.True);
        Assert.That(result.Items.All(c => c.TemplateId == null), Is.True,
            "GetDescendantsWithoutTemplatesAsync must not populate TemplateId");
        Assert.That(result.Items.All(c => c.PublishTemplateId == null), Is.True,
            "GetDescendantsWithoutTemplatesAsync must not populate PublishTemplateId");
    }

    [Test]
    public async Task GetDescendantsAsync_WithTemplate_PopulatesTemplateId()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Templated Descendant For Verify", _textpage.Id);
        content.TemplateId = _template.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["*"]);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            _textpage.Key, skip: 0, take: 100, ordering: null, CancellationToken.None);
        scope.Complete();

        IContent? templated = result.Items.FirstOrDefault(c => c.Key == content.Key);
        Assert.That(templated, Is.Not.Null);
        Assert.That(templated!.TemplateId, Is.EqualTo(_template.Id),
            "GetDescendantsAsync must populate TemplateId for content with a template assigned");
    }


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

    [Test]
    public async Task GetChildrenAsync_OrderedByName_WithCulture_UsesCultureVariantName()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        // Invariant names sort in opposite order from culture names — this proves the CCV join is used.
        // Invariant order: "A-Second" < "Z-First" → docB first.
        // Culture (en-US) order: "Alpha" < "Zeta" → docA first.
        var docA = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Z-First")
            .WithParentId(_textpage.Id)
            .Build();
        docA.SetCultureName("Alpha", "en-US");
        ContentService.Save(docA, -1);

        var docB = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("A-Second")
            .WithParentId(_textpage.Id)
            .Build();
        docB.SetCultureName("Zeta", "en-US");
        ContentService.Save(docB, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 10, propertyAliases: null,
            ordering: Ordering.By("name", culture: "en-US"),
            CancellationToken.None);
        scope.Complete();

        IContent first = result.Items.First(item => item.Key == docA.Key || item.Key == docB.Key);
        Assert.That(first.GetCultureName("en-US"), Is.EqualTo("Alpha"),
            "Culture name ordering must put 'Alpha' before 'Zeta', not fall back to invariant name order ('A-Second' before 'Z-First')");
    }

    [Test]
    public async Task GetDescendantsAsync_OrderedByName_WithCulture_UsesCultureVariantName()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        // Same inverted-name setup as the GetChildrenAsync variant.
        var docA = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Z-First")
            .WithParentId(_textpage.Id)
            .Build();
        docA.SetCultureName("Alpha", "en-US");
        ContentService.Save(docA, -1);

        var docB = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("A-Second")
            .WithParentId(_textpage.Id)
            .Build();
        docB.SetCultureName("Zeta", "en-US");
        ContentService.Save(docB, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            _textpage.Key, skip: 0, take: 10,
            ordering: Ordering.By("name", culture: "en-US"),
            CancellationToken.None);
        scope.Complete();

        IContent first = result.Items.First(item => item.Key == docA.Key || item.Key == docB.Key);
        Assert.That(first.GetCultureName("en-US"), Is.EqualTo("Alpha"),
            "Culture name ordering must put 'Alpha' before 'Zeta', not fall back to invariant name order ('A-Second' before 'Z-First')");
    }

    // --- Group 13: Custom property field ordering ---

    private async Task<IContentType> CreateIntPropertyContentTypeAsync()
    {
        var propertyCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "priority", ValueStorageType.Integer)
            {
                Alias = "priority",
                DataTypeId = -51,
            },
        };

        var contentType = ContentTypeBuilder.CreateBasicContentType("umbPriority", "Priority");
        contentType.PropertyGroups.Add(new PropertyGroup(propertyCollection) { Alias = "content", Name = "Content" });
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    [Test]
    public async Task GetChildrenAsync_OrderedByCustomIntProperty_OrdersByPropertyValue()
    {
        IContentType contentType = await CreateIntPropertyContentTypeAsync();

        var docHigh = new ContentBuilder().WithContentType(contentType).WithName("High").WithParentId(_textpage.Id).Build();
        docHigh.SetValue("priority", 30);
        ContentService.Save(docHigh, -1);

        var docLow = new ContentBuilder().WithContentType(contentType).WithName("Low").WithParentId(_textpage.Id).Build();
        docLow.SetValue("priority", 5);
        ContentService.Save(docLow, -1);

        var docMid = new ContentBuilder().WithContentType(contentType).WithName("Mid").WithParentId(_textpage.Id).Build();
        docMid.SetValue("priority", 15);
        ContentService.Save(docMid, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null,
            ordering: Ordering.By("priority", isCustomField: true),
            CancellationToken.None);
        scope.Complete();

        IContent[] custom = result.Items.Where(item => item.ContentType.Alias == contentType.Alias).ToArray();
        Assert.That(custom.Select(c => c.Key), Is.EqualTo(new[] { docLow.Key, docMid.Key, docHigh.Key }),
            "Ascending custom-field ordering should sort by the integer property value, low to high");
    }

    [Test]
    public async Task GetChildrenAsync_OrderedByCustomProperty_Descending_ReversesOrder()
    {
        IContentType contentType = await CreateIntPropertyContentTypeAsync();

        var docHigh = new ContentBuilder().WithContentType(contentType).WithName("High").WithParentId(_textpage.Id).Build();
        docHigh.SetValue("priority", 30);
        ContentService.Save(docHigh, -1);

        var docLow = new ContentBuilder().WithContentType(contentType).WithName("Low").WithParentId(_textpage.Id).Build();
        docLow.SetValue("priority", 5);
        ContentService.Save(docLow, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null,
            ordering: Ordering.By("priority", Direction.Descending, isCustomField: true),
            CancellationToken.None);
        scope.Complete();

        IContent[] custom = result.Items.Where(item => item.ContentType.Alias == contentType.Alias).ToArray();
        Assert.That(custom.Select(c => c.Key), Is.EqualTo(new[] { docHigh.Key, docLow.Key }),
            "Descending custom-field ordering should reverse the value order, high to low");
    }

    [Test]
    public async Task GetChildrenAsync_OrderedByCustomProperty_NodesWithoutValueSortFirst()
    {
        IContentType contentType = await CreateIntPropertyContentTypeAsync();

        var docWithValue = new ContentBuilder().WithContentType(contentType).WithName("HasValue").WithParentId(_textpage.Id).Build();
        docWithValue.SetValue("priority", 10);
        ContentService.Save(docWithValue, -1);

        // Force this node's SortOrder ahead of its siblings, so a fallback-to-SortOrder implementation
        // would (wrongly) place it first — only real custom-field ordering puts it last.
        docWithValue.SortOrder = -100;
        ContentService.Save(docWithValue, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 0, take: 100, propertyAliases: null,
            ordering: Ordering.By("priority", isCustomField: true),
            CancellationToken.None);
        scope.Complete();

        IContent[] children = result.Items.ToArray();
        int valueIndex = Array.FindIndex(children, c => c.Key == docWithValue.Key);
        Assert.That(valueIndex, Is.EqualTo(children.Length - 1),
            "The only node with a 'priority' value should sort last — siblings with no value for the custom field must sort first ascending");
    }

    [Test]
    public async Task GetDescendantsAsync_OrderedByCustomProperty_OrdersByPropertyValue()
    {
        IContentType contentType = await CreateIntPropertyContentTypeAsync();

        var docHigh = new ContentBuilder().WithContentType(contentType).WithName("High").WithParentId(_subpage.Id).Build();
        docHigh.SetValue("priority", 30);
        ContentService.Save(docHigh, -1);

        var docLow = new ContentBuilder().WithContentType(contentType).WithName("Low").WithParentId(_textpage.Id).Build();
        docLow.SetValue("priority", 5);
        ContentService.Save(docLow, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetDescendantsAsync(
            _textpage.Key, skip: 0, take: 100,
            ordering: Ordering.By("priority", isCustomField: true),
            CancellationToken.None);
        scope.Complete();

        IContent[] custom = result.Items.Where(item => item.ContentType.Alias == contentType.Alias).ToArray();
        Assert.That(custom.Select(c => c.Key), Is.EqualTo(new[] { docLow.Key, docHigh.Key }),
            "Custom-field ordering must apply across the whole descendant tree, not just direct children");
    }

    [Test]
    public async Task GetChildrenAsync_OrderedByCustomProperty_WithPaging_ReturnsCorrectPage()
    {
        IContentType contentType = await CreateIntPropertyContentTypeAsync();

        // Created out of value order (3, 1, 2) so SortOrder (creation order) disagrees with the
        // expected value order — a fallback-to-SortOrder implementation would land on the wrong node.
        var doc3 = new ContentBuilder().WithContentType(contentType).WithName("Three").WithParentId(_textpage.Id).Build();
        doc3.SetValue("priority", 30);
        ContentService.Save(doc3, -1);

        var doc1 = new ContentBuilder().WithContentType(contentType).WithName("One").WithParentId(_textpage.Id).Build();
        doc1.SetValue("priority", 10);
        ContentService.Save(doc1, -1);

        var doc2 = new ContentBuilder().WithContentType(contentType).WithName("Two").WithParentId(_textpage.Id).Build();
        doc2.SetValue("priority", 20);
        ContentService.Save(doc2, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        // Full ascending order is: _subpage, _subpage2 (no value), then doc1=10, doc2=20, doc3=30.
        PagedModel<IContent> result = await repository.GetChildrenAsync(
            _textpage.Key, skip: 3, take: 1, propertyAliases: null,
            ordering: Ordering.By("priority", isCustomField: true),
            CancellationToken.None);
        scope.Complete();

        Assert.That(result.Total, Is.EqualTo(5), "Total should count all children regardless of ordering");
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.Single().Key, Is.EqualTo(doc2.Key),
            "Skip=3 should land on doc2 once the two valueless siblings and doc1 are skipped");
    }

    // Creates a content type with a single property using the REAL DateTimeWithTimeZone property editor —
    // the one editor in the codebase implementing IDataValueSortable — so SortableValue population can be
    // exercised end-to-end through the repository write path (not through ContentEditingService/ContentService).
    private async Task<IContentType> CreateSortableDateTimePropertyContentTypeAsync()
    {
        PropertyEditorCollection propertyEditors = GetRequiredService<PropertyEditorCollection>();
        IDataEditor propertyEditor = propertyEditors[Constants.PropertyEditors.Aliases.DateTimeWithTimeZone];

        var dataType = new DataType(propertyEditor, GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            Name = "DateTime With TimeZone (Sortable Test)",
            DatabaseType = ValueStorageType.Ntext,
        };
        Attempt<IDataType, DataTypeOperationStatus> dataTypeResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeResult.Success, $"Failed to create data type: {dataTypeResult.Status}");

        var propertyCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "eventDate", ValueStorageType.Ntext)
            {
                Alias = "eventDate",
                DataTypeId = dataTypeResult.Result.Id,
            },
        };

        var contentType = ContentTypeBuilder.CreateBasicContentType("umbEventDate", "EventDate");
        contentType.PropertyGroups.Add(new PropertyGroup(propertyCollection) { Alias = "content", Name = "Content" });
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    [Test]
    public async Task PersistNewItemAsync_SortablePropertyEditor_PopulatesSortableValue()
    {
        IContentType contentType = await CreateSortableDateTimePropertyContentTypeAsync();

        // Storage-format value (not editor format) — a JSON-serialized DateTimeDto, matching what
        // DateTimeDataValueEditor.FromEditor would have produced.
        var eventDate = new DateTimeOffset(2024, 3, 15, 13, 30, 0, TimeSpan.FromHours(2));
        var dateTimeDto = new DateTimeValueConverterBase.DateTimeDto { Date = eventDate };
        IJsonSerializer jsonSerializer = GetRequiredService<IJsonSerializer>();
        string storageValue = jsonSerializer.Serialize(dateTimeDto);

        var content = new ContentBuilder().WithContentType(contentType).WithName("Sortable Event").WithParentId(_textpage.Id).Build();
        content.SetValue("eventDate", storageValue);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        PropertyDataDto? propertyData = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.PropertyData.FirstOrDefaultAsync(pd => pd.VersionId == content.VersionId));

        scope.Complete();

        string expectedSortableValue = eventDate.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture);

        Assert.That(propertyData, Is.Not.Null);
        Assert.That(propertyData!.SortableValue, Is.EqualTo(expectedSortableValue),
            "SortableValue must be populated on write for a property whose editor implements IDataValueSortable");
    }

    [Test]
    public async Task PersistNewItemAsync_InvariantUnpublishedWithProperties_PersistsAndReadsBack()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "New Page", _textpage.Id);
        content.SetValue("title", "Some Value");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.HasIdentity, Is.True);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.ParentId, Is.EqualTo(_textpage.Id));
            Assert.That(result.Level, Is.EqualTo(2));
            Assert.That(result.Path, Is.EqualTo($"{_textpage.Path},{result.Id}"));
            Assert.That(result.Published, Is.False);
            Assert.That(result.Edited, Is.True);
            Assert.That(result.GetValue<string>("title"), Is.EqualTo("Some Value"),
                "the property value must round-trip through PropertyData insertion");
        });
    }

    [Test]
    public async Task PersistNewItemAsync_AssignsRootParentPathAndLevel_WhenParentIsRoot()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Root Page", -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.ParentId, Is.EqualTo(-1));
            Assert.That(result.Level, Is.EqualTo(1));
            Assert.That(result.Path, Is.EqualTo($"-1,{result.Id}"));
        });
    }

    [Test]
    public async Task PersistNewItemAsync_SortOrderCollision_AssignsNextAvailableSortOrder()
    {
        var parent = ContentBuilder.CreateSimpleContent(_contentType, "Sort Order Parent", -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await repository.SaveAsync(parent, CancellationToken.None);

        var first = ContentBuilder.CreateSimpleContent(_contentType, "First Sibling", parent.Id);
        first.SortOrder = 5;
        await repository.SaveAsync(first, CancellationToken.None);

        var second = ContentBuilder.CreateSimpleContent(_contentType, "Second Sibling", parent.Id);
        second.SortOrder = 5;
        await repository.SaveAsync(second, CancellationToken.None);

        scope.Complete();

        Assert.That(first.SortOrder, Is.EqualTo(5));
        Assert.That(second.SortOrder, Is.EqualTo(6),
            "second save should detect the SortOrder collision with its sibling and bump to the next available slot");
    }

    [Test]
    public async Task PersistNewItemAsync_FiresContentRefreshNotification()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        var repository = new AsyncDocumentRepository(
            GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            AppCaches.Disabled,
            LoggerFactory,
            GetRequiredService<ILanguageRepository>(),
            GetRequiredService<IRelationRepository>(),
            GetRequiredService<IRelationTypeRepository>(),
            GetRequiredService<PropertyEditorCollection>(),
            GetRequiredService<DataValueReferenceFactoryCollection>(),
            GetRequiredService<IDataTypeService>(),
            eventAggregatorMock.Object,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>(),
            GetRequiredService<IContentTypeRepository>(),
            GetRequiredService<ITemplateRepository>(),
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ITagRepository>(),
            GetRequiredService<IJsonSerializer>());

        var content = ContentBuilder.CreateSimpleContent(_contentType, "Notify Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        await repository.SaveAsync(content, CancellationToken.None);
        scope.Complete();

        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<ContentRefreshNotification>()), Times.Once);
    }

    [Test]
    public async Task PersistNewItemAsync_DefaultTemplateAssigned_WhenNoTemplateSpecified()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Template Default Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TemplateId, Is.EqualTo(_template.Id),
            "the content type's default template should be assigned when no template was explicitly set");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_NoDirtyProperties_ReturnsWithoutError()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "No Change Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        DateTime originalUpdateDate = content.UpdateDate;

        await repository.SaveAsync(content, CancellationToken.None);
        scope.Complete();

        Assert.That(content.UpdateDate, Is.EqualTo(originalUpdateDate),
            "a no-op save must not touch UpdateDate, proving the early-return guard skipped the write");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_ChangesName_PersistsAndReadsBack()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Original Name", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        DateTime originalUpdateDate = content.UpdateDate;

        content.Name = "Updated Name";
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Name, Is.EqualTo("Updated Name"));
            Assert.That(content.UpdateDate, Is.GreaterThan(originalUpdateDate));
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_AddsNewPropertyValue_PersistsValue()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Add Property Page", _textpage, setPropertyValues: false);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        content.SetValue("title", "Newly Added Value");
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetValue<string>("title"), Is.EqualTo("Newly Added Value"),
            "a property with no prior PropertyData row must be inserted (toInsert branch)");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_ChangesExistingPropertyValue_PersistsNewValue()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Change Property Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        content.SetValue("title", "Changed Value");
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetValue<string>("title"), Is.EqualTo("Changed Value"),
            "an existing PropertyData row must be updated in place (toUpdate branch)");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_RemovesPropertyValue_DeletesRow()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Remove Property Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        content.SetValue("title", null); // clear the value entirely
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.GetValue<string>("title"), Is.Null.Or.Empty,
            "clearing a property's value must delete the orphaned PropertyData row (delete branch)");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_ParentIdDirty_RecomputesPathLevelSortOrder()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Move Me Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        content.ParentId = _subpage.Id;
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.ParentId, Is.EqualTo(_subpage.Id));
            Assert.That(result.Level, Is.EqualTo(3));
            Assert.That(result.Path, Is.EqualTo($"{_subpage.Path},{result.Id}"));
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_NonCurrentVersion_ThrowsInvalidOperationException()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Stale Version Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions
                .Where(contentVersion => contentVersion.Id == content.VersionId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(contentVersion => contentVersion.Current, false)));

        content.Name = "Renamed After Going Stale";

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repository.SaveAsync(content, CancellationToken.None));

        scope.Complete();
    }

    [Test]
    public async Task PersistUpdatedItemAsync_FiresContentRefreshNotification()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Notify Update Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await repository.SaveAsync(content, CancellationToken.None);

        var eventAggregatorMock = new Mock<IEventAggregator>();
        var notifyingRepository = new AsyncDocumentRepository(
            GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            AppCaches.Disabled,
            LoggerFactory,
            GetRequiredService<ILanguageRepository>(),
            GetRequiredService<IRelationRepository>(),
            GetRequiredService<IRelationTypeRepository>(),
            GetRequiredService<PropertyEditorCollection>(),
            GetRequiredService<DataValueReferenceFactoryCollection>(),
            GetRequiredService<IDataTypeService>(),
            eventAggregatorMock.Object,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>(),
            GetRequiredService<IContentTypeRepository>(),
            GetRequiredService<ITemplateRepository>(),
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ITagRepository>(),
            GetRequiredService<IJsonSerializer>());

        content.Name = "Notify Update Page Renamed";
        await notifyingRepository.SaveAsync(content, CancellationToken.None);
        scope.Complete();

        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<ContentRefreshNotification>()), Times.Once);
    }

    [Test]
    public async Task PersistNewItemAsync_CultureVariant_PersistsNamesPerCulture()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();
        IContent content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("English Name", "en-US");
        content.SetCultureName("Nom Français", "fr");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.GetCultureName("en-US"), Is.EqualTo("English Name"));
            Assert.That(result.GetCultureName("fr"), Is.EqualTo("Nom Français"));
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_CultureVariant_ChangesNameForOneCulture_LeavesOthersUnchanged()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();
        IContent content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("English Name", "en-US");
        content.SetCultureName("Nom Français", "fr");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        content.SetCultureName("Nom Modifié", "fr");
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.GetCultureName("fr"), Is.EqualTo("Nom Modifié"));
            Assert.That(result.GetCultureName("en-US"), Is.EqualTo("English Name"),
                "changing one culture's name must not affect the other culture's persisted name");
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_CultureVariant_RemovesACulture_DeletesVariationRows()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();
        IContent content = ContentBuilder.CreateBasicContent(contentType);
        content.SetCultureName("English Name", "en-US");
        content.SetCultureName("Nom Français", "fr");

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        int versionId = content.VersionId;
        int nodeId = content.Id;

        content.SetCultureName(null, "fr");
        await repository.SaveAsync(content, CancellationToken.None);

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);

        List<ContentVersionCultureVariationDto> contentVariations = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersionCultureVariations.Where(variation => variation.VersionId == versionId).ToListAsync());

        List<DocumentCultureVariationDto> entityVariations = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.DocumentCultureVariations.Where(variation => variation.NodeId == nodeId).ToListAsync());

        scope.Complete();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AvailableCultures, Does.Not.Contain("fr"),
                "the removed culture must no longer be reported as available after read-back");
            Assert.That(result.GetCultureName("fr"), Is.Null);
            Assert.That(result.GetCultureName("en-US"), Is.EqualTo("English Name"));
            Assert.That(contentVariations, Has.Count.EqualTo(1),
                "only en-US's ContentVersionCultureVariation row should remain for this version");
            Assert.That(entityVariations, Has.Count.EqualTo(1),
                "only en-US's DocumentCultureVariation row should remain for this node");
        });
    }

    [Test]
    public async Task PersistNewItemAsync_PublishOnCreate_WritesTwoVersionRowPairs()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Publish On Create Page", _textpage.Id);
        content.PublishedState = PublishedState.Publishing;

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;

        List<ContentVersionDto> contentVersions = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.Where(contentVersion => contentVersion.NodeId == nodeId).ToListAsync());

        List<DocumentVersionDto> documentVersions = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.DocumentVersions.Where(documentVersion => contentVersions.Select(cv => cv.Id).Contains(documentVersion.Id)).ToListAsync());

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.That(contentVersions, Has.Count.EqualTo(2), "publishing on create must write two ContentVersion rows");
        Assert.That(documentVersions, Has.Count.EqualTo(2), "publishing on create must write two DocumentVersion rows");

        ContentVersionDto publishedContentVersion = contentVersions.Single(cv => cv.Current == false);
        ContentVersionDto draftContentVersion = contentVersions.Single(cv => cv.Current);

        Assert.That(publishedContentVersion.Key, Is.Not.EqualTo(draftContentVersion.Key),
            "the two version rows must have distinct Keys, not a duplicated Key from the first row");

        DocumentVersionDto publishedDocumentVersion = documentVersions.Single(dv => dv.Id == publishedContentVersion.Id);
        DocumentVersionDto draftDocumentVersion = documentVersions.Single(dv => dv.Id == draftContentVersion.Id);

        Assert.Multiple(() =>
        {
            Assert.That(publishedDocumentVersion.Published, Is.True, "the Current=false row must be the Published=true row");
            Assert.That(draftDocumentVersion.Published, Is.False, "the Current=true row must be Published=false");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Published, Is.True);
            Assert.That(result.VersionId, Is.Not.EqualTo(result.PublishedVersionId),
                "VersionId (draft) and PublishedVersionId must be distinct after publish-on-create");
            Assert.That(result.PublishedVersionId, Is.EqualTo(publishedContentVersion.Id));
            Assert.That(result.VersionId, Is.EqualTo(draftContentVersion.Id));

            // The original in-memory instance (not a fresh GetAsync re-fetch) must also reflect the
            // publish — a caller that inspects `content` right after SaveAsync returns, without
            // re-fetching, should see the same state as a fresh read.
            Assert.That(content.Published, Is.True);
            Assert.That(content.PublishDate, Is.Not.Null);
            Assert.That(content.PublisherId, Is.EqualTo(content.WriterId));
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_Publish_UnpublishesOldRowAndInsertsNewDraft()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Publish Then Republish Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        // Step 1: save as a plain draft.
        await repository.SaveAsync(content, CancellationToken.None);
        int originalDraftVersionId = content.VersionId;

        // Step 2: first publish - exercises the simple "no prior published version" case.
        content.PublishedState = PublishedState.Publishing;
        await repository.SaveAsync(content, CancellationToken.None);
        int firstPublishedVersionId = content.PublishedVersionId;
        int secondDraftVersionId = content.VersionId;

        Assert.That(firstPublishedVersionId, Is.EqualTo(originalDraftVersionId),
            "the original draft row becomes the first published row");

        // Step 3: change something, then publish again - exercises the "unpublish the old published
        // version" branch, since a prior published version now exists.
        content.SetValue("title", "Changed for second publish");
        content.PublishedState = PublishedState.Publishing;
        await repository.SaveAsync(content, CancellationToken.None);
        int secondPublishedVersionId = content.PublishedVersionId;
        int thirdDraftVersionId = content.VersionId;

        // firstPublishedVersionId == originalDraftVersionId (asserted above) - the original draft row
        // became the first published row in step 2, and must now be unpublished (superseded) by step 3.
        DocumentVersionDto firstPublishedRow = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.DocumentVersions.FirstAsync(documentVersion => documentVersion.Id == firstPublishedVersionId));

        DocumentVersionDto secondPublishedRow = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.DocumentVersions.FirstAsync(documentVersion => documentVersion.Id == secondPublishedVersionId));

        DocumentVersionDto thirdDraftRow = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.DocumentVersions.FirstAsync(documentVersion => documentVersion.Id == thirdDraftVersionId));

        ContentVersionDto secondPublishedContentVersion = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.FirstAsync(contentVersion => contentVersion.Id == secondPublishedVersionId));

        ContentVersionDto thirdDraftContentVersion = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.FirstAsync(contentVersion => contentVersion.Id == thirdDraftVersionId));

        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(firstPublishedRow.Published, Is.False,
                "a prior published version must be unpublished once a newer version is published");
            Assert.That(secondPublishedRow.Published, Is.True, "the second-publish row must be the currently published version");
            Assert.That(secondPublishedContentVersion.Current, Is.False);
            Assert.That(thirdDraftRow.Published, Is.False, "a brand new draft row must exist and not be published");
            Assert.That(thirdDraftContentVersion.Current, Is.True);
            Assert.That(secondPublishedVersionId, Is.Not.EqualTo(thirdDraftVersionId));
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_Unpublish_SetsPublishedFalseNoNewVersionRow()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Unpublish Page", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);

        content.PublishedState = PublishedState.Publishing;
        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;

        int rowCountAfterPublish = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.CountAsync(contentVersion => contentVersion.NodeId == nodeId));

        content.PublishedState = PublishedState.Unpublishing;
        await repository.SaveAsync(content, CancellationToken.None);

        int rowCountAfterUnpublish = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.CountAsync(contentVersion => contentVersion.NodeId == nodeId));

        IContent? result = await repository.GetAsync(content.Key, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Published, Is.False);
            Assert.That(rowCountAfterUnpublish, Is.EqualTo(rowCountAfterPublish),
                "unpublishing must not create a new ContentVersion/DocumentVersion row pair");
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_PublishWithTags_PersistsTagRelationships()
    {
        var contentType = ContentTypeBuilder.CreateSimpleTagsContentType("umbTags", "Tags Page", defaultTemplateId: _template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged Page", _textpage.Id);
        content.SetValue("tags", "[\"red\",\"blue\"]");
        content.PublishedState = PublishedState.Publishing;

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;

        List<string> tagTexts = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.TagRelationships.Where(tagRelationship => tagRelationship.NodeId == nodeId)
                .Join(db.Tags, tagRelationship => tagRelationship.TagId, tag => tag.Id, (tagRelationship, tag) => tag.Text)
                .ToListAsync());

        scope.Complete();

        Assert.That(tagTexts, Is.EquivalentTo(new[] { "red", "blue" }),
            "publishing content with a tags property must persist tag relationships via SetEntityTags");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_PublishExistingDraftWithTags_PersistsTagRelationships()
    {
        var contentType = ContentTypeBuilder.CreateSimpleTagsContentType("umbTagsRepublish", "Tags Republish Page", defaultTemplateId: _template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged Draft Page", _textpage.Id);
        content.SetValue("tags", "[\"yellow\",\"purple\"]");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        // First save: HasIdentity is false and PublishedState is still the default Unpublished, so this
        // routes through PersistNewItemAsync, giving the entity an identity as a plain draft.
        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;

        // Second save: HasIdentity is now true, so this routes through PersistUpdatedItemAsync with
        // publishing == true — the specific "publish an existing draft via update" path.
        content.PublishedState = PublishedState.Publishing;
        await repository.SaveAsync(content, CancellationToken.None);

        List<string> tagTexts = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.TagRelationships.Where(tagRelationship => tagRelationship.NodeId == nodeId)
                .Join(db.Tags, tagRelationship => tagRelationship.TagId, tag => tag.Id, (tagRelationship, tag) => tag.Text)
                .ToListAsync());

        scope.Complete();

        Assert.That(tagTexts, Is.EquivalentTo(new[] { "yellow", "purple" }),
            "publishing an existing draft via PersistUpdatedItemAsync must persist tag relationships via SetEntityTags");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_PublishExistingDraftWithTags_TagsVisibleBeforeRefreshNotification()
    {
        var contentType = ContentTypeBuilder.CreateSimpleTagsContentType("umbTagsRepublishOrder", "Tags Republish Order Page", defaultTemplateId: _template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged Draft Order Page", _textpage.Id);
        content.SetValue("tags", "[\"orange\"]");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;

        // Captures the tag relationships visible at the exact moment the refresh notification fires —
        // distinguishes the mid-function SetEntityTags call (before the notification) from the later
        // duplicate call in ApplyPostPublishFlagFlipsAsync (after it), which cache-populating
        // notification handlers rely on seeing up-to-date tags synchronously.
        List<string>? tagTextsAtRefreshTime = null;
        var eventAggregatorMock = new Mock<IEventAggregator>();
        eventAggregatorMock
            .Setup(x => x.Publish(It.IsAny<ContentRefreshNotification>()))
            .Callback(() =>
            {
                tagTextsAtRefreshTime = scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
                        db.TagRelationships.Where(tagRelationship => tagRelationship.NodeId == nodeId)
                            .Join(db.Tags, tagRelationship => tagRelationship.TagId, tag => tag.Id, (tagRelationship, tag) => tag.Text)
                            .ToListAsync())
                    .GetAwaiter().GetResult();
            });

        var notifyingRepository = new AsyncDocumentRepository(
            GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            AppCaches.Disabled,
            LoggerFactory,
            GetRequiredService<ILanguageRepository>(),
            GetRequiredService<IRelationRepository>(),
            GetRequiredService<IRelationTypeRepository>(),
            GetRequiredService<PropertyEditorCollection>(),
            GetRequiredService<DataValueReferenceFactoryCollection>(),
            GetRequiredService<IDataTypeService>(),
            eventAggregatorMock.Object,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>(),
            GetRequiredService<IContentTypeRepository>(),
            GetRequiredService<ITemplateRepository>(),
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ITagRepository>(),
            GetRequiredService<IJsonSerializer>());

        content.PublishedState = PublishedState.Publishing;
        await notifyingRepository.SaveAsync(content, CancellationToken.None);
        scope.Complete();

        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<ContentRefreshNotification>()), Times.Once);
        Assert.That(tagTextsAtRefreshTime, Is.EquivalentTo(new[] { "orange" }),
            "tag relationships must already be persisted by the time the content refresh notification fires");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_UnpublishWithTags_ClearsTagRelationships()
    {
        var contentType = ContentTypeBuilder.CreateSimpleTagsContentType("umbTagsUnpublish", "Tags Unpublish Page", defaultTemplateId: _template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged Page For Unpublish", _textpage.Id);
        content.SetValue("tags", "[\"green\"]");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        content.PublishedState = PublishedState.Publishing;
        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;

        content.PublishedState = PublishedState.Unpublishing;
        await repository.SaveAsync(content, CancellationToken.None);

        List<TagRelationshipDto> remaining = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.TagRelationships.Where(tagRelationship => tagRelationship.NodeId == nodeId).ToListAsync());

        scope.Complete();

        Assert.That(remaining, Is.Empty,
            "unpublishing must clear tag relationships via ClearEntityTags");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_IsMoving_SkipsVersioningAndPropertyDataButUpdatesNodePathAndLevel()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        // A descendant several levels below the node actually being moved: only its Path, Level and
        // (via UpdatingEntity) UpdateDate change during a bulk move — its ParentId is untouched, since
        // its immediate parent didn't change, only some ancestor further up did. This is the exact
        // dirty-property combination IsMoving() checks for.
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Bulk Move Descendant Page", _subpage.Id);
        content.SetValue("title", "Original Value");

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        int nodeId = content.Id;
        int versionId = content.VersionId;
        int originalParentId = content.ParentId;

        ContentVersionDto originalContentVersion = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.FirstAsync(contentVersion => contentVersion.Id == versionId));

        List<PropertyDataDto> originalPropertyData = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.PropertyData.Where(propertyData => propertyData.VersionId == versionId).OrderBy(propertyData => propertyData.Id).ToListAsync());

        // Simulate the effect of ContentService.PerformMoveDescendantLocked on this descendant: Path and
        // Level are set directly, ParentId is left alone.
        content.Path = $"{_subpage2.Path},{_subpage.Id},{content.Id}";
        content.Level = _subpage2.Level + 2;

        await repository.SaveAsync(content, CancellationToken.None);
        scope.Complete();

        NodeDto node = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.Nodes.FirstAsync(n => n.NodeId == nodeId));

        ContentVersionDto contentVersionAfterMove = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.FirstAsync(contentVersion => contentVersion.Id == versionId));

        List<PropertyDataDto> propertyDataAfterMove = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.PropertyData.Where(propertyData => propertyData.VersionId == versionId).OrderBy(propertyData => propertyData.Id).ToListAsync());

        int versionCountAfterMove = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.CountAsync(contentVersion => contentVersion.NodeId == nodeId));

        Assert.Multiple(() =>
        {
            Assert.That(node.Path, Is.EqualTo(content.Path), "the Node row's Path must still be updated on the fast path");
            Assert.That(node.Level, Is.EqualTo(content.Level), "the Node row's Level must still be updated on the fast path");
            Assert.That(node.ParentId, Is.EqualTo(originalParentId), "ParentId must be untouched — a descendant's immediate parent doesn't change during a bulk move");

            Assert.That(versionCountAfterMove, Is.EqualTo(1), "a move must not create a new ContentVersion/DocumentVersion row pair");
            Assert.That(contentVersionAfterMove.VersionDate, Is.EqualTo(originalContentVersion.VersionDate),
                "ContentVersion.VersionDate must be untouched on the fast path — proves the version-update block was skipped");
            Assert.That(contentVersionAfterMove.Text, Is.EqualTo(originalContentVersion.Text));

            Assert.That(propertyDataAfterMove, Has.Count.EqualTo(originalPropertyData.Count));
            for (var i = 0; i < originalPropertyData.Count; i++)
            {
                Assert.That(propertyDataAfterMove[i].Id, Is.EqualTo(originalPropertyData[i].Id),
                    "PropertyData rows must be untouched (same primary keys) on the fast path");
                Assert.That(propertyDataAfterMove[i].VarcharValue, Is.EqualTo(originalPropertyData[i].VarcharValue));
            }

            Assert.That(content.IsDirty(), Is.False, "a successful save must reset dirty properties on the fast path too");
        });
    }

    [Test]
    public async Task PersistUpdatedItemAsync_IsMoving_FiresContentRefreshNotification()
    {
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Bulk Move Notify Page", _subpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await repository.SaveAsync(content, CancellationToken.None);

        var eventAggregatorMock = new Mock<IEventAggregator>();
        var notifyingRepository = new AsyncDocumentRepository(
            GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            AppCaches.Disabled,
            LoggerFactory,
            GetRequiredService<ILanguageRepository>(),
            GetRequiredService<IRelationRepository>(),
            GetRequiredService<IRelationTypeRepository>(),
            GetRequiredService<PropertyEditorCollection>(),
            GetRequiredService<DataValueReferenceFactoryCollection>(),
            GetRequiredService<IDataTypeService>(),
            eventAggregatorMock.Object,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>(),
            GetRequiredService<IContentTypeRepository>(),
            GetRequiredService<ITemplateRepository>(),
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ITagRepository>(),
            GetRequiredService<IJsonSerializer>());

        content.Path = $"{_subpage2.Path},{_subpage.Id},{content.Id}";
        content.Level = _subpage2.Level + 2;
        await notifyingRepository.SaveAsync(content, CancellationToken.None);
        scope.Complete();

        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<ContentRefreshNotification>()), Times.Once,
            "OnUowRefreshedEntityAsync must still fire on the fast path");
    }

    [Test]
    public async Task PersistUpdatedItemAsync_PathDirtyButLevelNotDirty_IsNotTreatedAsMoving_GeneralPathStillRuns()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        var content = ContentBuilder.CreateSimpleContent(_contentType, "Not Actually Moving Page", _subpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(content, CancellationToken.None);
        int versionId = content.VersionId;

        ContentVersionDto originalContentVersion = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.FirstAsync(contentVersion => contentVersion.Id == versionId));

        // Only Path becomes dirty here — Level is left alone, so IsMoving() must be false and the
        // general (non-fast) path must still run, even though a "move-like" property changed.
        // ValidatePath only checks the last two segments against ParentId, so prefixing an extra
        // (unvalidated) segment yields a Path that is both different (dirty) and still valid.
        content.Path = $"0,{content.Path}";
        content.Name = "Renamed While Not Moving";

        await repository.SaveAsync(content, CancellationToken.None);

        ContentVersionDto contentVersionAfterSave = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.ContentVersions.FirstAsync(contentVersion => contentVersion.Id == versionId));

        scope.Complete();

        Assert.That(contentVersionAfterSave.VersionDate, Is.GreaterThan(originalContentVersion.VersionDate),
            "with Level not dirty, IsMoving() must be false and the general path (which updates VersionDate) must run");
        Assert.That(contentVersionAfterSave.Text, Is.EqualTo("Renamed While Not Moving"));
    }

    // --- Group 20: Recycle bin ---

    [Test]
    public async Task GetRecycleBinAsync_ReturnsAllTrashedItemsRegardlessOfDepth()
    {
        // _trashed (from SetUpData) is already Trashed=true with ParentId = -20 (a direct child of the
        // recycle bin). Add a deep descendant that is ALSO trashed but whose ParentId points at _trashed,
        // not -20 — GetRecycleBinAsync mirrors NPoco's ContentRepositoryBase.GetRecycleBin (a flat
        // Trashed-only filter), so it must include both, not just direct children of the bin.
        var deepDescendant = ContentBuilder.CreateSimpleContent(_contentType, "Deep Trashed Descendant", _trashed.Id);
        deepDescendant.Trashed = true;
        ContentService.Save(deepDescendant, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> result = await repository.GetRecycleBinAsync(CancellationToken.None);
        scope.Complete();

        IContent[] items = result.ToArray();
        Assert.That(items.Any(c => c.Key == _trashed.Key), Is.True);
        Assert.That(items.Any(c => c.Key == deepDescendant.Key), Is.True,
            "GetRecycleBinAsync must include trashed descendants that aren't direct children of the bin");
    }

    [Test]
    public async Task GetRecycleBinAsync_ExcludesNonTrashedItems()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        IEnumerable<IContent> result = await repository.GetRecycleBinAsync(CancellationToken.None);
        scope.Complete();

        IContent[] items = result.ToArray();
        Assert.That(items.Any(c => c.Key == _trashed.Key), Is.True);
        Assert.That(items.Any(c => c.Key == _textpage.Key), Is.False);
        Assert.That(items.Any(c => c.Key == _subpage.Key), Is.False);
    }

    [Test]
    public async Task GetPagedRecycleBinAsync_ReturnsPagedTrashedItemsWithTotal()
    {
        // Trash two more items (in addition to the existing _trashed) so paging has something to page over.
        _subpage.Trashed = true;
        ContentService.Save(_subpage, -1);
        _subpage2.Trashed = true;
        ContentService.Save(_subpage2, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> firstPage = await repository.GetPagedRecycleBinAsync(
            pageIndex: 0, pageSize: 2, ordering: null, CancellationToken.None);
        PagedModel<IContent> secondPage = await repository.GetPagedRecycleBinAsync(
            pageIndex: 1, pageSize: 2, ordering: null, CancellationToken.None);
        scope.Complete();

        Assert.That(firstPage.Total, Is.EqualTo(3));
        Assert.That(firstPage.Items.Count(), Is.EqualTo(2));
        Assert.That(secondPage.Total, Is.EqualTo(3));
        Assert.That(secondPage.Items.Count(), Is.EqualTo(1), "page 2 of a 3-item set with pageSize=2 has exactly 1 remaining item");

        Guid[] allKeys = firstPage.Items.Select(c => c.Key).Concat(secondPage.Items.Select(c => c.Key)).ToArray();
        Assert.That(allKeys, Is.EquivalentTo(new[] { _trashed.Key, _subpage.Key, _subpage2.Key }),
            "the two pages together must cover all 3 trashed items with no duplicates/omissions");
    }

    [Test]
    public async Task GetPagedRecycleBinAsync_OrderedByInvariantName_SortsTrashedItems()
    {
        _subpage.Trashed = true;
        _subpage.Name = "Zzz Last";
        ContentService.Save(_subpage, -1);
        _subpage2.Trashed = true;
        _subpage2.Name = "Aaa First";
        ContentService.Save(_subpage2, -1);
        _trashed.Name = "Mmm Middle";
        ContentService.Save(_trashed, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        // No culture, so Ordering.IsInvariant is true and this exercises the plain node.Text ordering
        // path (FetchDefaultOrdered), not the culture-variant name path.
        PagedModel<IContent> result = await repository.GetPagedRecycleBinAsync(
            pageIndex: 0, pageSize: 10, ordering: Ordering.By("name"), CancellationToken.None);
        scope.Complete();

        IContent[] items = result.Items.ToArray();
        Assert.That(items, Has.Length.EqualTo(3));
        Assert.That(items.Select(c => c.Name), Is.EqualTo(new[] { "Aaa First", "Mmm Middle", "Zzz Last" }));
    }

    [Test]
    public async Task GetPagedRecycleBinAsync_OrderedByName_WithCulture_UsesCultureVariantName()
    {
        IContentType contentType = await CreateVariantContentTypeAsync();

        // Invariant names sort in opposite order from culture names — this proves the CCV join is used,
        // mirroring GetChildrenAsync_OrderedByName_WithCulture_UsesCultureVariantName.
        var docA = new ContentBuilder().WithContentType(contentType).WithName("Z-First").WithParentId(_textpage.Id).Build();
        docA.SetCultureName("Alpha", "en-US");
        docA.Trashed = true;
        ContentService.Save(docA, -1);

        var docB = new ContentBuilder().WithContentType(contentType).WithName("A-Second").WithParentId(_textpage.Id).Build();
        docB.SetCultureName("Zeta", "en-US");
        docB.Trashed = true;
        ContentService.Save(docB, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetPagedRecycleBinAsync(
            pageIndex: 0, pageSize: 10, ordering: Ordering.By("name", culture: "en-US"), CancellationToken.None);
        scope.Complete();

        IContent first = result.Items.First(item => item.Key == docA.Key || item.Key == docB.Key);
        Assert.That(first.GetCultureName("en-US"), Is.EqualTo("Alpha"),
            "Culture name ordering must put 'Alpha' before 'Zeta', not fall back to invariant name order ('A-Second' before 'Z-First')");
    }

    [Test]
    public async Task GetPagedRecycleBinAsync_OrderedByCustomIntProperty_OrdersByPropertyValue()
    {
        IContentType contentType = await CreateIntPropertyContentTypeAsync();

        var docHigh = new ContentBuilder().WithContentType(contentType).WithName("High").WithParentId(_textpage.Id).Build();
        docHigh.SetValue("priority", 30);
        docHigh.Trashed = true;
        ContentService.Save(docHigh, -1);

        var docLow = new ContentBuilder().WithContentType(contentType).WithName("Low").WithParentId(_textpage.Id).Build();
        docLow.SetValue("priority", 5);
        docLow.Trashed = true;
        ContentService.Save(docLow, -1);

        var docMid = new ContentBuilder().WithContentType(contentType).WithName("Mid").WithParentId(_textpage.Id).Build();
        docMid.SetValue("priority", 15);
        docMid.Trashed = true;
        ContentService.Save(docMid, -1);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        PagedModel<IContent> result = await repository.GetPagedRecycleBinAsync(
            pageIndex: 0, pageSize: 100, ordering: Ordering.By("priority", isCustomField: true), CancellationToken.None);
        scope.Complete();

        IContent[] custom = result.Items.Where(item => item.ContentType.Alias == contentType.Alias).ToArray();
        Assert.That(custom.Select(c => c.Key), Is.EqualTo(new[] { docLow.Key, docMid.Key, docHigh.Key }),
            "Ascending custom-field ordering should sort by the integer property value, low to high");
    }

    [Test]
    public async Task RecycleBinSmellsAsync_WhenRecycleBinHasDirectChild_ReturnsTrue()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        bool smells = await repository.RecycleBinSmellsAsync(CancellationToken.None);
        scope.Complete();

        Assert.That(smells, Is.True, "_trashed is a direct child of the recycle bin (-20) per SetUpData");
    }

    [Test]
    public async Task RecycleBinSmellsAsync_IgnoresTrashedNodesThatAreNotDirectChildrenOfTheBin()
    {
        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();

        using var scope = NewScopeProvider.CreateScope();

        // Reparent the only trashed node away from being a direct child of the recycle bin (-20), while
        // leaving it Trashed = true — simulating a trashed node RecycleBinSmells must NOT count, since
        // NPoco's equivalent (CountChildren(RecycleBinId)) only checks direct children of the bin, unlike
        // GetRecycleBinAsync's flat Trashed-only filter.
        await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.Nodes.Where(node => node.NodeId == _trashed.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(node => node.ParentId, _textpage.Id)));

        var repository = CreateRepository();
        bool smells = await repository.RecycleBinSmellsAsync(CancellationToken.None);
        scope.Complete();

        Assert.That(smells, Is.False);
    }
}
