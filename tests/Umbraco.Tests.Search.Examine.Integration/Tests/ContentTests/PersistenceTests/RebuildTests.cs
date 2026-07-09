using Umbraco.Cms.Tests.Integration.Testing;
using System.Diagnostics;
using System.Reflection;
using Examine;
using Examine.Lucene.Providers;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Tests.Search.Examine.Integration.Attributes;
using Umbraco.Tests.Search.Examine.Integration.Extensions;
using Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;
using Umbraco.Tests.Search.Integration;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.PersistenceTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class RebuildTests : UmbracoIntegrationTest
{
    private bool _indexingComplete;

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IIndexDocumentRepository IndexDocumentRepository => GetRequiredService<IIndexDocumentRepository>();

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    private IContent _rootDocument = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddExamineSearchProviderForTest<TestIndex, TestInMemoryDirectoryFactory>();

        builder.AddSearchCore();

        builder.Services.AddUnique<IBackgroundTaskQueue, ImmediateBackgroundTaskQueue>();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder.Services.AddUnique<IServerEventRouter, NoOpServerEventRouter>();
        builder.AddNotificationHandler<LanguageCacheRefresherNotification, RebuildIndexesNotificationHandler>();

        // the core ConfigureBuilderAttribute won't execute from other assemblies at the moment, so this is a workaround
        var testType = Type.GetType(TestContext.CurrentContext.Test.ClassName!);
        if (testType is not null)
        {
            MethodInfo? methodInfo = testType.GetMethod(TestContext.CurrentContext.Test.Name);
            if (methodInfo is not null)
            {
                foreach (ConfigureUmbracoBuilderAttribute attribute in methodInfo.GetCustomAttributes(typeof(ConfigureUmbracoBuilderAttribute), true).OfType<ConfigureUmbracoBuilderAttribute>())
                {
                    attribute.Execute(builder, testType);
                }
            }
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task RebuildPersistsDocumentsToDatabaseIfNotExisting(bool publish)
    {
        await CreateContentWithPersistence(publish);
        var indexAlias = GetIndexAlias(publish);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify document exists in database (from initial indexing)
            IndexDocument? rootDocInitial = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocInitial, Is.Not.Null);

            // Delete the database entry to simulate a fresh state (e.g., after migration or database restore)
            await IndexDocumentRepository.DeleteAsync([_rootDocument.Key], publish);

            // Verify document no longer exists in database
            IndexDocument? rootDocBefore = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocBefore, Is.Null);
        }

        // Trigger rebuild
        ContentIndexingService.Rebuild(indexAlias, TestBase.DefaultOrigin);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify document now exists in database again (rebuilt from content)
            IndexDocument? rootDocAfter = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocAfter, Is.Not.Null);
            Assert.That(FieldsContainText(rootDocAfter!.Fields, "Root Document"), Is.True);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task RebuildUsesExistingDatabaseEntries(bool publish)
    {
        await CreateContentWithPersistence(publish);
        var indexAlias = GetIndexAlias(publish);

        IndexField[] rootFieldsBefore;
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify document exists in database
            IndexDocument? rootDocBefore = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocBefore, Is.Not.Null);

            rootFieldsBefore = rootDocBefore!.Fields;
        }

        // Trigger rebuild
        ContentIndexingService.Rebuild(indexAlias, TestBase.DefaultOrigin);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify document still exists and fields are the same (fetched from DB, not recalculated)
            IndexDocument? rootDocAfter = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);

            Assert.That(rootDocAfter, Is.Not.Null);
            Assert.That(rootDocAfter.Fields.Length, Is.GreaterThan(0));
            Assert.That(rootDocAfter.Fields.Length, Is.EqualTo(rootFieldsBefore.Length));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task RebuildFromMemory_RecalculatesFields(bool publish)
    {
        await CreateContentWithPersistence(publish);
        var indexAlias = GetIndexAlias(publish);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify document exists in database with original name
            IndexDocument? rootDocBefore = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocBefore, Is.Not.Null);
            Assert.That(FieldsContainText(rootDocBefore!.Fields, "Root Document"), Is.True);
        }

        // Update the content name directly (simulating a change)
        _rootDocument.Name = "Updated Document Name";
        await WaitForIndexing(indexAlias, () =>
        {
            ContentService.Save(_rootDocument);
            if (publish)
            {
                ContentService.Publish(_rootDocument, ["*"]);
            }

            return Task.CompletedTask;
        });

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify the database now has the updated name
            IndexDocument? rootDocUpdated = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocUpdated, Is.Not.Null);
            Assert.That(FieldsContainText(rootDocUpdated!.Fields, "Updated Document Name"), Is.True);

            await IndexDocumentRepository.DeleteAsync([_rootDocument.Key], publish);
        }

        // Trigger rebuild with useDatabase=false (should recalculate, not use stale DB data)
        ContentIndexingService.Rebuild(indexAlias, TestBase.DefaultOrigin);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify the database now has fresh recalculated fields with the actual content name
            IndexDocument? rootDocAfter = await IndexDocumentRepository.GetAsync(_rootDocument.Key, publish);
            Assert.That(rootDocAfter, Is.Not.Null);
            Assert.That(FieldsContainText(rootDocAfter!.Fields, "Updated Document Name"), Is.True);
            Assert.That(FieldsContainText(rootDocAfter.Fields, "Root Document"), Is.False);
        }
    }

    private string GetIndexAlias(bool publish) => publish ? Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent : Umbraco.Cms.Core.Constants.IndexAliases.DraftContent;

    /// <summary>
    /// Creates content and waits for indexing (so database persistence happens).
    /// </summary>
    private async Task CreateContentWithPersistence(bool publish)
    {
        // Create content type
        ContentTypeCreateModel contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(
            "testType",
            "Test Type");
        Attempt<IContentType?, ContentTypeOperationStatus> contentTypeAttempt = await ContentTypeEditingService.CreateAsync(
            contentTypeCreateModel,
            Umbraco.Cms.Core.Constants.Security.SuperUserKey);
        Assert.That(contentTypeAttempt.Success, Is.True);
        IContentType contentType = contentTypeAttempt.Result!;

        var indexAlias = GetIndexAlias(publish);

        // Create root content with indexing
        ContentCreateModel rootCreateModel = ContentEditingBuilder.CreateSimpleContent(contentType.Key, "Root Document");
        await WaitForIndexing(indexAlias, async () =>
        {
            Attempt<ContentCreateResult, ContentEditingOperationStatus> createRootResult = await ContentEditingService.CreateAsync(rootCreateModel, Umbraco.Cms.Core.Constants.Security.SuperUserKey);
            Assert.That(createRootResult.Success, Is.True);
            _rootDocument = createRootResult.Result.Content!;

            if (publish)
            {
                ContentService.Publish(_rootDocument, ["*"]);
            }
        });
    }

    protected async Task WaitForIndexing(string indexAlias, Func<Task> indexUpdatingAction)
    {
        var activeIndexManager = GetRequiredService<IActiveIndexManager>();
        var physicalName = activeIndexManager.IsRebuilding(indexAlias)
            ? activeIndexManager.ResolveShadowIndexName(indexAlias)
            : activeIndexManager.ResolveActiveIndexName(indexAlias);
        var index = (LuceneIndex)GetRequiredService<IExamineManager>().GetIndex(physicalName);
        index.IndexCommitted += IndexCommited;

        var hasDoneAction = false;

        var stopWatch = Stopwatch.StartNew();

        while (_indexingComplete is false)
        {
            if (hasDoneAction is false)
            {
                await indexUpdatingAction();
                hasDoneAction = true;
            }

            if (stopWatch.ElapsedMilliseconds > 10000)
            {
                throw new TimeoutException("Indexing timed out");
            }

            await Task.Delay(250);
        }

        _indexingComplete = false;
        index.IndexCommitted -= IndexCommited;
    }

    private void IndexCommited(object? sender, EventArgs e) => _indexingComplete = true;

    private static bool FieldsContainText(IndexField[] fields, string text)
        => fields.Any(f =>
            (f.Value.Texts?.Any(t => t.Contains(text)) == true) ||
            (f.Value.TextsR1?.Any(t => t.Contains(text)) == true) ||
            (f.Value.TextsR2?.Any(t => t.Contains(text)) == true) ||
            (f.Value.TextsR3?.Any(t => t.Contains(text)) == true) ||
            (f.Value.Keywords?.Any(k => k.Contains(text)) == true));
}
