using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Integration tests that verify the end-to-end behavior of
/// <see cref="DocumentUrlServiceContentTreeChangeNotificationHandler"/>:
/// the handler writes URL segments and aliases to the database on publish,
/// while the cache-only variants (<c>UpdateUrlSegmentCacheAsync</c> /
/// <c>UpdateAliasCacheAsync</c> etc.) leave the database unchanged.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class DocumentUrlServiceContentTreeChangeTests : UmbracoIntegrationTest
{
    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    private IDocumentUrlAliasService DocumentUrlAliasService => GetRequiredService<IDocumentUrlAliasService>();

    private IDocumentUrlRepository DocumentUrlRepository => GetRequiredService<IDocumentUrlRepository>();

    private IDocumentUrlAliasRepository DocumentUrlAliasRepository => GetRequiredService<IDocumentUrlAliasRepository>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ContentType ContentType { get; set; } = null!;

    private Content RootPage { get; set; } = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlAliasServiceInitializerNotificationHandler>();
        // DocumentUrlServiceContentTreeChangeNotificationHandler is globally registered via UmbracoBuilder.cs
    }

    [SetUp]
    public async Task SetUpTestData()
    {
        await DocumentUrlService.InitAsync(false, CancellationToken.None);
        await DocumentUrlAliasService.InitAsync(false, CancellationToken.None);

        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        ContentType = CreateContentTypeWithUrlAlias(template.Id);
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        RootPage = ContentBuilder.CreateSimpleContent(ContentType, "Root Page");
        ContentService.Save(RootPage, -1);
        ContentService.Publish(RootPage, []);
    }

    private ContentType CreateContentTypeWithUrlAlias(int templateId)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("testPageWithAlias")
            .WithName("Test Page With Alias")
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithSortOrder(1)
                    .Done()
                .AddPropertyType()
                    .WithAlias("bodyText")
                    .WithName("Body text")
                    .WithSortOrder(2)
                    .Done()
                .AddPropertyType()
                    .WithAlias("author")
                    .WithName("Author")
                    .WithSortOrder(3)
                    .Done()
                .AddPropertyType()
                    .WithAlias(Constants.Conventions.Content.UrlAlias)
                    .WithName("URL Alias")
                    .WithSortOrder(4)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .Done()
                .Done()
            .AddAllowedTemplate()
                .WithId(templateId)
                .WithAlias("defaultTemplate")
                .WithName("Default Template")
                .Done()
            .WithDefaultTemplateId(templateId)
            .Build();

        return (ContentType)contentType;
    }

    private List<PublishedDocumentUrlSegment> GetDbSegments(Guid documentKey)
    {
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return DocumentUrlRepository.GetAll()
                .Where(r => r.DocumentKey == documentKey)
                .ToList();
        }
    }

    private List<PublishedDocumentUrlAlias> GetDbAliases(Guid documentKey)
    {
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return DocumentUrlAliasRepository.GetAll()
                .Where(r => r.DocumentKey == documentKey)
                .ToList();
        }
    }

    /// <summary>
    /// After publishing a document the notification handler must have written URL segments to
    /// the database without any manual call to <c>CreateOrUpdateUrlSegmentsAsync</c>.
    /// </summary>
    [Test]
    public void Publish_WritesUrlSegmentsToDatabase_ViaNotificationHandler()
    {
        var page = ContentBuilder.CreateSimpleContent(ContentType, "Test Page", RootPage.Id);
        ContentService.Save(page, -1);
        ContentService.Publish(page, []);

        var rows = GetDbSegments(page.Key);

        Assert.That(rows, Is.Not.Empty,
            "The ContentTreeChangeNotification handler must write URL segments to the database on publish.");
    }

    /// <summary>
    /// After publishing a document with a URL alias, the notification handler must have written
    /// the alias to the database without any manual call to <c>CreateOrUpdateAliasesAsync</c>.
    /// </summary>
    [Test]
    public void Publish_WithUrlAlias_WritesAliasesToDatabase_ViaNotificationHandler()
    {
        var page = ContentBuilder.CreateSimpleContent(ContentType, "Alias Page", RootPage.Id);
        page.SetValue(Constants.Conventions.Content.UrlAlias, "my-integration-alias");
        ContentService.Save(page, -1);
        ContentService.Publish(page, []);

        var aliases = GetDbAliases(page.Key);

        Assert.That(aliases, Is.Not.Empty,
            "The ContentTreeChangeNotification handler must write URL aliases to the database on publish.");
        Assert.That(aliases.Any(a => a.Alias == "my-integration-alias"), Is.True,
            "The persisted alias value must match what was set on the content.");
    }

    /// <summary>
    /// Publishing a branch must write URL segments for every node in the subtree to the database,
    /// covering the <c>RefreshBranch</c> path in the notification handler.
    /// </summary>
    [Test]
    public void PublishBranch_WritesUrlSegmentsForAllDescendants_ViaNotificationHandler()
    {
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "Parent", RootPage.Id);
        ContentService.Save(parent, -1);

        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child", parent.Id);
        ContentService.Save(child, -1);

        var grandchild = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild", child.Id);
        ContentService.Save(grandchild, -1);

        ContentService.PublishBranch(parent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.Multiple(() =>
        {
            Assert.That(GetDbSegments(parent.Key), Is.Not.Empty,
                "Parent URL segments must be in the database after PublishBranch.");
            Assert.That(GetDbSegments(child.Key), Is.Not.Empty,
                "Child URL segments must be in the database after PublishBranch.");
            Assert.That(GetDbSegments(grandchild.Key), Is.Not.Empty,
                "Grandchild URL segments must be in the database after PublishBranch.");
        });
    }

    /// <summary>
    /// <see cref="IDocumentUrlService.UpdateUrlSegmentCacheAsync"/> must not write additional rows
    /// to the database. After calling it the row count for the document key must be unchanged, but
    /// the in-memory <c>GetUrlSegment</c> lookup must still work.
    /// </summary>
    [Test]
    public async Task UpdateUrlSegmentCacheAsync_DoesNotWriteAdditionalRowsToDatabase_ButUpdatesCache()
    {
        var page = ContentBuilder.CreateSimpleContent(ContentType, "URL Cache Test Page", RootPage.Id);
        ContentService.Save(page, -1);
        ContentService.Publish(page, []);

        var rowsBefore = GetDbSegments(page.Key);
        Assert.That(rowsBefore, Is.Not.Empty, "Pre-condition: publish must have written segments.");

        await DocumentUrlService.UpdateUrlSegmentCacheAsync(page.Key);

        var rowsAfter = GetDbSegments(page.Key);

        Assert.That(rowsAfter.Count, Is.EqualTo(rowsBefore.Count),
            "UpdateUrlSegmentCacheAsync must not write additional rows to the database.");

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;
        var segment = DocumentUrlService.GetUrlSegment(page.Key, isoCode, isDraft: false);
        Assert.That(segment, Is.Not.Null,
            "UpdateUrlSegmentCacheAsync must keep the in-memory cache populated.");
    }

    /// <summary>
    /// <see cref="IDocumentUrlAliasService.UpdateAliasCacheAsync"/> must not write additional rows
    /// to the database. After <see cref="IDocumentUrlAliasService.DeleteAliasesFromCacheAsync"/>
    /// evicts the alias, calling <c>UpdateAliasCacheAsync</c> must restore the cache without
    /// touching the database.
    /// </summary>
    [Test]
    public async Task UpdateAliasCacheAsync_DoesNotWriteAdditionalRowsToDatabase_ButResolvesAlias()
    {
        var page = ContentBuilder.CreateSimpleContent(ContentType, "Alias Cache Page", RootPage.Id);
        page.SetValue(Constants.Conventions.Content.UrlAlias, "cache-test-alias");
        ContentService.Save(page, -1);
        ContentService.Publish(page, []);

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var aliasesBefore = GetDbAliases(page.Key);
        Assert.That(aliasesBefore, Is.Not.Empty, "Pre-condition: publish must have written aliases.");

        // Evict from cache so lookup fails.
        await DocumentUrlAliasService.DeleteAliasesFromCacheAsync(new[] { page.Key });
        Assert.That(
            await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("cache-test-alias", isoCode),
            Is.Empty,
            "Pre-condition: alias must not be resolvable after cache eviction.");

        await DocumentUrlAliasService.UpdateAliasCacheAsync(page.Key);

        var aliasesAfter = GetDbAliases(page.Key);
        Assert.That(aliasesAfter.Count, Is.EqualTo(aliasesBefore.Count),
            "UpdateAliasCacheAsync must not write additional alias rows to the database.");

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("cache-test-alias", isoCode);
        Assert.That(result, Does.Contain(page.Key),
            "UpdateAliasCacheAsync must restore the in-memory alias cache.");
    }

    /// <summary>
    /// <see cref="IDocumentUrlService.UpdateUrlSegmentCacheWithDescendantsAsync"/> must not write
    /// additional rows to the database for any node in the subtree.
    /// </summary>
    [Test]
    public async Task UpdateUrlSegmentCacheWithDescendantsAsync_DoesNotWriteAdditionalRowsToDatabase()
    {
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "Parent Cache Test", RootPage.Id);
        ContentService.Save(parent, -1);

        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child Cache Test", parent.Id);
        ContentService.Save(child, -1);

        ContentService.PublishBranch(parent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        var parentRowsBefore = GetDbSegments(parent.Key);
        var childRowsBefore = GetDbSegments(child.Key);
        Assert.That(parentRowsBefore, Is.Not.Empty, "Pre-condition: parent segments must be in DB.");
        Assert.That(childRowsBefore, Is.Not.Empty, "Pre-condition: child segments must be in DB.");

        await DocumentUrlService.UpdateUrlSegmentCacheWithDescendantsAsync(parent.Key);

        var parentRowsAfter = GetDbSegments(parent.Key);
        var childRowsAfter = GetDbSegments(child.Key);

        Assert.Multiple(() =>
        {
            Assert.That(parentRowsAfter.Count, Is.EqualTo(parentRowsBefore.Count),
                "UpdateUrlSegmentCacheWithDescendantsAsync must not write additional rows for the parent.");
            Assert.That(childRowsAfter.Count, Is.EqualTo(childRowsBefore.Count),
                "UpdateUrlSegmentCacheWithDescendantsAsync must not write additional rows for the child.");
        });

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;
        Assert.Multiple(() =>
        {
            Assert.That(DocumentUrlService.GetUrlSegment(parent.Key, isoCode, isDraft: false), Is.Not.Null,
                "Parent URL segment must still be resolvable from in-memory cache.");
            Assert.That(DocumentUrlService.GetUrlSegment(child.Key, isoCode, isDraft: false), Is.Not.Null,
                "Child URL segment must still be resolvable from in-memory cache.");
        });
    }

    /// <summary>
    /// <see cref="IDocumentUrlAliasService.UpdateAliasCacheWithDescendantsAsync"/> must restore the
    /// in-memory cache for both the root and all descendants without writing to the database.
    /// </summary>
    [Test]
    public async Task UpdateAliasCacheWithDescendantsAsync_DoesNotWriteAdditionalRowsToDatabase_ButUpdatesCache()
    {
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "Alias Parent", RootPage.Id);
        parent.SetValue(Constants.Conventions.Content.UrlAlias, "parent-alias");
        ContentService.Save(parent, -1);

        var child = ContentBuilder.CreateSimpleContent(ContentType, "Alias Child", parent.Id);
        child.SetValue(Constants.Conventions.Content.UrlAlias, "child-alias");
        ContentService.Save(child, -1);

        ContentService.PublishBranch(parent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var parentAliasesBefore = GetDbAliases(parent.Key);
        var childAliasesBefore = GetDbAliases(child.Key);
        Assert.That(parentAliasesBefore, Is.Not.Empty, "Pre-condition: parent alias must be in DB.");
        Assert.That(childAliasesBefore, Is.Not.Empty, "Pre-condition: child alias must be in DB.");

        // Evict both from cache.
        await DocumentUrlAliasService.DeleteAliasesFromCacheAsync(new[] { parent.Key, child.Key });

        await DocumentUrlAliasService.UpdateAliasCacheWithDescendantsAsync(parent.Key);

        var parentAliasesAfter = GetDbAliases(parent.Key);
        var childAliasesAfter = GetDbAliases(child.Key);

        Assert.Multiple(() =>
        {
            Assert.That(parentAliasesAfter.Count, Is.EqualTo(parentAliasesBefore.Count),
                "UpdateAliasCacheWithDescendantsAsync must not write additional rows for the parent.");
            Assert.That(childAliasesAfter.Count, Is.EqualTo(childAliasesBefore.Count),
                "UpdateAliasCacheWithDescendantsAsync must not write additional rows for the child.");
        });

        var parentResult = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("parent-alias", isoCode);
        var childResult = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("child-alias", isoCode);

        Assert.Multiple(() =>
        {
            Assert.That(parentResult, Does.Contain(parent.Key),
                "Parent alias must be resolvable after UpdateAliasCacheWithDescendantsAsync.");
            Assert.That(childResult, Does.Contain(child.Key),
                "Child alias must be resolvable after UpdateAliasCacheWithDescendantsAsync.");
        });
    }
}
