using Examine;
using Examine.Lucene.Providers;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Tests.Search.Examine.Integration.Tests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ZeroDowntimeReindexingTests : TestBase
{
    private IActiveIndexManager ActiveIndexManager => GetRequiredService<IActiveIndexManager>();

    private IExamineManager ExamineManager => GetRequiredService<IExamineManager>();

    private IIndexer Indexer => GetRequiredService<IIndexer>();

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private Guid _createdContentKey;

    [TestCase(true)]
    [TestCase(false)]
    public async Task IndexerWritesToActiveIndex_WhenNotRebuilding(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        Assert.That(ActiveIndexManager.IsRebuilding(indexAlias), Is.False);

        var activePhysicalName = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        IIndex activeIndex = ExamineManager.GetIndex(activePhysicalName);
        ISearchResults results = activeIndex.Searcher.CreateQuery().All().Execute();

        Assert.That(results.TotalItemCount, Is.GreaterThan(0), "Active index should contain documents from normal indexing");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task IndexerDoesNotWriteToShadowIndex_WhenNotRebuilding(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        Assert.That(ActiveIndexManager.IsRebuilding(indexAlias), Is.False);

        var shadowPhysicalName = ActiveIndexManager.ResolveShadowIndexName(indexAlias);
        IIndex shadowIndex = ExamineManager.GetIndex(shadowPhysicalName);

        if (shadowIndex is IIndexStats stats)
        {
            Assert.That(stats.GetDocumentCount(), Is.EqualTo(0), "Shadow index should be empty when not rebuilding");
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task IndexerWritesToShadowIndex_WhenRebuilding(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        // Start rebuilding and reset shadow
        ActiveIndexManager.StartRebuilding(indexAlias);
        await Indexer.ResetAsync(indexAlias);

        // During rebuild, a content change should write to shadow
        await WaitForShadowIndexing(indexAlias, () =>
        {
            IContent content = ContentService.GetById(_createdContentKey)!;
            content.Name = "Updated During Rebuild";
            ContentService.Save(content);
            if (publish)
            {
                ContentService.Publish(content, ["*"]);
            }

            return Task.CompletedTask;
        });

        // Verify shadow index has data
        var shadowPhysicalName = ActiveIndexManager.ResolveShadowIndexName(indexAlias);
        IIndex shadowIndex = ExamineManager.GetIndex(shadowPhysicalName);
        ISearchResults shadowResults = shadowIndex.Searcher.CreateQuery().All().Execute();

        Assert.That(shadowResults.TotalItemCount, Is.GreaterThan(0), "Shadow index should contain documents when rebuilding");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SearcherAlwaysReadsFromActiveIndex(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        var activePhysicalName = ActiveIndexManager.ResolveActiveIndexName(indexAlias);

        IIndex activeIndex = ExamineManager.GetIndex(activePhysicalName);
        var countBeforeRebuild = activeIndex.Searcher.CreateQuery().All().Execute().TotalItemCount;
        Assert.That(countBeforeRebuild, Is.GreaterThan(0));

        // Start rebuilding
        ActiveIndexManager.StartRebuilding(indexAlias);

        // Active index name should remain the same during rebuild
        var activePhysicalNameDuringRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        Assert.That(activePhysicalNameDuringRebuild, Is.EqualTo(activePhysicalName), "Active physical name should not change while rebuilding");

        // Active index should still be queryable with same data
        IIndex activeIndexDuringRebuild = ExamineManager.GetIndex(activePhysicalNameDuringRebuild);
        var countDuringRebuild = activeIndexDuringRebuild.Searcher.CreateQuery().All().Execute().TotalItemCount;
        Assert.That(countDuringRebuild, Is.EqualTo(countBeforeRebuild), "Active index document count should remain unchanged during rebuild");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task NewIndexDataGetsAddedToShadow_WhenRebuilding(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);
        var shadowBeforeRebuild = ActiveIndexManager.ResolveShadowIndexName(indexAlias);

        // Start rebuild and write data to shadow
        ActiveIndexManager.StartRebuilding(indexAlias);

        await WaitForShadowIndexing(indexAlias, () =>
        {
            IContent content = ContentService.GetById(_createdContentKey)!;
            content.Name = "Rebuilt Content";
            ContentService.Save(content);
            if (publish)
            {
                ContentService.Publish(content, ["*"]);
            }

            return Task.CompletedTask;
        });

        // Complete the rebuild (swap)
        ActiveIndexManager.CompleteRebuilding(indexAlias);

        // Verify the swap happened
        var activeAfterRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        Assert.That(activeAfterRebuild, Is.EqualTo(shadowBeforeRebuild), "Active should have swapped to what was shadow");

        // Verify the new active index has data
        IIndex newActiveIndex = ExamineManager.GetIndex(activeAfterRebuild);
        ISearchResults results = newActiveIndex.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.GreaterThan(0), "New active index should contain documents after swap");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CancelledRebuild_KeepsOriginalActiveIndex(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        var activeBeforeRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);

        ActiveIndexManager.StartRebuilding(indexAlias);
        Assert.That(ActiveIndexManager.IsRebuilding(indexAlias), Is.True);

        ActiveIndexManager.CancelRebuilding(indexAlias);

        Assert.Multiple(() =>
        {
            Assert.That(ActiveIndexManager.IsRebuilding(indexAlias), Is.False);
            Assert.That(ActiveIndexManager.ResolveActiveIndexName(indexAlias), Is.EqualTo(activeBeforeRebuild), "Active index should not change after cancelled rebuild");
        });

        // Active index should still be queryable
        IIndex activeIndex = ExamineManager.GetIndex(activeBeforeRebuild);
        ISearchResults results = activeIndex.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.GreaterThan(0), "Active index should still be queryable after cancelled rebuild");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task GetMetadata_ReturnsRebuilding_DuringRebuild(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        IndexMetadata metadataBefore = await Indexer.GetMetadataAsync(indexAlias);
        Assert.That(metadataBefore.HealthStatus, Is.EqualTo(HealthStatus.Healthy));

        ActiveIndexManager.StartRebuilding(indexAlias);
        IndexMetadata metadataDuring = await Indexer.GetMetadataAsync(indexAlias);
        Assert.That(metadataDuring.HealthStatus, Is.EqualTo(HealthStatus.Rebuilding));

        ActiveIndexManager.CancelRebuilding(indexAlias);
        IndexMetadata metadataAfter = await Indexer.GetMetadataAsync(indexAlias);
        Assert.That(metadataAfter.HealthStatus, Is.Not.EqualTo(HealthStatus.Rebuilding));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task GetMetadata_ReportsActiveIndexDocumentCount_DuringRebuild(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        IndexMetadata metadataBefore = await Indexer.GetMetadataAsync(indexAlias);
        Assert.That(metadataBefore.DocumentCount, Is.GreaterThan(0));

        ActiveIndexManager.StartRebuilding(indexAlias);
        IndexMetadata metadataDuring = await Indexer.GetMetadataAsync(indexAlias);
        Assert.That(metadataDuring.DocumentCount, Is.EqualTo(metadataBefore.DocumentCount), "During rebuild, metadata should report active index document count");
    }

    [Test]
    public async Task DifferentIndexes_HaveIndependentRebuildState()
    {
        await SetUpContent(publish: true);

        var publishedAlias = Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;
        var draftAlias = Umbraco.Cms.Core.Constants.IndexAliases.DraftContent;

        ActiveIndexManager.StartRebuilding(publishedAlias);

        Assert.Multiple(() =>
        {
            Assert.That(ActiveIndexManager.IsRebuilding(publishedAlias), Is.True, "Published index should be rebuilding");
            Assert.That(ActiveIndexManager.IsRebuilding(draftAlias), Is.False, "Draft index should not be rebuilding");
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task DoubleSwap_ReturnsToOriginalSlot(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        var activeInitial = ActiveIndexManager.ResolveActiveIndexName(indexAlias);

        // First swap
        ActiveIndexManager.StartRebuilding(indexAlias);
        ActiveIndexManager.CompleteRebuilding(indexAlias);

        var activeAfterFirstSwap = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        Assert.That(activeAfterFirstSwap, Is.Not.EqualTo(activeInitial), "First swap should change active");

        // Second swap
        ActiveIndexManager.StartRebuilding(indexAlias);
        ActiveIndexManager.CompleteRebuilding(indexAlias);

        var activeAfterSecondSwap = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        Assert.That(activeAfterSecondSwap, Is.EqualTo(activeInitial), "Second swap should return to original slot");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task FullRebuild_CompletesWithoutLeavingRebuildingState(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        Assert.That(ActiveIndexManager.IsRebuilding(indexAlias), Is.False);

        // Trigger full rebuild (synchronous with ImmediateBackgroundTaskQueue)
        ContentIndexingService.Rebuild(indexAlias, DefaultOrigin);

        // After the synchronous rebuild, IsRebuilding should be false
        Assert.That(ActiveIndexManager.IsRebuilding(indexAlias), Is.False, "Rebuild should not still be in progress after synchronous completion");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task FullRebuild_SwapsActiveAndShadowIndexes(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        var activeBeforeRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        var shadowBeforeRebuild = ActiveIndexManager.ResolveShadowIndexName(indexAlias);

        // Trigger full rebuild (synchronous with ImmediateBackgroundTaskQueue).
        // The ZeroDowntimeRebuildNotificationHandler should detect the healthy shadow and swap.
        ContentIndexingService.Rebuild(indexAlias, DefaultOrigin);

        var activeAfterRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);

        Assert.Multiple(() =>
        {
            Assert.That(activeAfterRebuild, Is.EqualTo(shadowBeforeRebuild), "Active index should have swapped to the former shadow after rebuild");
            Assert.That(ActiveIndexManager.ResolveShadowIndexName(indexAlias), Is.EqualTo(activeBeforeRebuild), "Shadow index should have swapped to the former active after rebuild");
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task FullRebuild_ClearsShadowIndexAfterSwap(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        var activeBeforeRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);

        // Trigger full rebuild (synchronous with ImmediateBackgroundTaskQueue).
        ContentIndexingService.Rebuild(indexAlias, DefaultOrigin);

        // Verify the swap happened
        var activeAfterRebuild = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        Assert.That(activeAfterRebuild, Is.Not.EqualTo(activeBeforeRebuild), "Active index should have swapped after rebuild");

        // After the rebuild and swap, the shadow index (old active) should be cleared to free disk space
        var shadowPhysicalName = ActiveIndexManager.ResolveShadowIndexName(indexAlias);
        Assert.That(shadowPhysicalName, Is.EqualTo(activeBeforeRebuild), "Shadow should be the old active");

        IIndex shadowIndex = ExamineManager.GetIndex(shadowPhysicalName);

        if (shadowIndex is IIndexStats stats)
        {
            Assert.That(stats.GetDocumentCount(), Is.EqualTo(0), "Shadow index should be cleared after a successful rebuild and swap");
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task IndexerReset_TargetsShadowDuringRebuild(bool publish)
    {
        await SetUpContent(publish);
        var indexAlias = GetIndexAlias(publish);

        var activePhysicalName = ActiveIndexManager.ResolveActiveIndexName(indexAlias);

        IIndex activeIndex = ExamineManager.GetIndex(activePhysicalName);
        var activeCountBefore = activeIndex.Searcher.CreateQuery().All().Execute().TotalItemCount;
        Assert.That(activeCountBefore, Is.GreaterThan(0));

        // Start rebuilding and reset (reset should target shadow, not active)
        ActiveIndexManager.StartRebuilding(indexAlias);
        await Indexer.ResetAsync(indexAlias);

        // Active index should be untouched
        var activeCountAfterReset = activeIndex.Searcher.CreateQuery().All().Execute().TotalItemCount;
        Assert.That(activeCountAfterReset, Is.EqualTo(activeCountBefore), "Active index should not be affected by reset during rebuild");

        ActiveIndexManager.CancelRebuilding(indexAlias);
    }

    private async Task SetUpContent(bool publish)
    {
        ContentTypeCreateModel contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(
            "testZeroDowntime",
            "Test Zero Downtime");
        Attempt<IContentType?, ContentTypeOperationStatus> contentTypeAttempt = await ContentTypeEditingService.CreateAsync(
            contentTypeCreateModel,
            Umbraco.Cms.Core.Constants.Security.SuperUserKey);
        Assert.That(contentTypeAttempt.Success, Is.True);
        IContentType contentType = contentTypeAttempt.Result!;

        ContentCreateModel rootCreateModel = ContentEditingBuilder.CreateSimpleContent(contentType.Key, "Zero Downtime Test Content");
        var indexAlias = GetIndexAlias(publish);

        await WaitForIndexing(indexAlias, async () =>
        {
            Attempt<ContentCreateResult, ContentEditingOperationStatus> createResult = await ContentEditingService.CreateAsync(
                rootCreateModel,
                Umbraco.Cms.Core.Constants.Security.SuperUserKey);
            Assert.That(createResult.Success, Is.True);

            _createdContentKey = createResult.Result.Content!.Key;

            if (publish)
            {
                ContentService.Publish(createResult.Result.Content!, ["*"]);
            }
        });
    }

    /// <summary>
    /// Waits for the shadow index to commit after performing an action during rebuild.
    /// Unlike <see cref="TestBase.WaitForIndexing"/>, this targets the shadow index.
    /// </summary>
    private async Task WaitForShadowIndexing(string indexAlias, Func<Task> indexUpdatingAction)
    {
        var shadowPhysicalName = ActiveIndexManager.ResolveShadowIndexName(indexAlias);
        var index = (LuceneIndex)ExamineManager.GetIndex(shadowPhysicalName);

        var committed = false;
        void OnCommitted(object? sender, EventArgs e) => committed = true;
        index.IndexCommitted += OnCommitted;

        try
        {
            await indexUpdatingAction();

            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            while (!committed)
            {
                if (stopWatch.ElapsedMilliseconds > 4000)
                {
                    throw new TimeoutException("Shadow index commit timed out");
                }

                await Task.Delay(250);
            }
        }
        finally
        {
            index.IndexCommitted -= OnCommitted;
        }
    }
}
