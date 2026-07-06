using System.Collections.Concurrent;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;
using IUmbracoBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Search;

/// <summary>
/// Drives real element operations through the full notification -> element cache refresher -> reindex pipeline over a
/// nested Document -> Outer element -> Inner element structure, and asserts the content that would be written to the
/// external index. A capturing <see cref="IUmbracoIndexingHandler"/> records the published value set the real builder
/// produces for each reindexed document, so we verify both that the right document is reindexed and what it contains.
/// </summary>
internal sealed class ElementReindexTriggerTests : BlockEditorWithReusableContentTestBase
{
    private const string InnerText = "the inner element text";
    private const string OuterText = "the outer element text";

    private static readonly ConcurrentDictionary<int, ValueSet> _indexed = new();

    private DeferredSearchReindexService ReindexService
        => (DeferredSearchReindexService)GetRequiredService<IDeferredSearchReindexService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementPublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();

        builder.Services.Configure<IndexingSettings>(config => config.IndexExternalBlockElements = true);

        _indexed.Clear();
        builder.Services.AddUnique<IUmbracoIndexingHandler>(sp =>
            new CapturingIndexingHandler(sp.GetRequiredService<IPublishedContentValueSetBuilder>(), _indexed));
    }

    [Test]
    public async Task Baseline_Document_Indexes_Both_Outer_And_Inner_Content()
    {
        var nest = await CreateNestedStructure();

        var blocks = await ReindexAndGetDocumentBlocks(nest);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(blocks.Contains(OuterText), "The document should index the outer element's content.");
            Assert.IsTrue(blocks.Contains(InnerText), "The document should index the inner element's content transitively.");
        });
    }

    [Test]
    public async Task Publishing_Changed_Inner_Reindexes_Document_With_New_Content()
    {
        var nest = await CreateNestedStructure();
        const string newInnerText = "the updated inner element text";

        await ResetTracking();
        await UpdateInvariantText(nest.InnerKey, newInnerText);
        await PublishElement(nest.InnerKey);

        var blocks = GetDocumentBlocks(nest.Document.Id);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(blocks.Contains(newInnerText), "Publishing the inner element should refresh the document with its new content.");
            Assert.IsFalse(blocks.Contains(InnerText), "The document should no longer contain the old inner content.");
        });
    }

    [Test]
    public async Task Draft_Saving_Inner_Does_Not_Reindex_Document()
    {
        var nest = await CreateNestedStructure();

        await ResetTracking();
        await UpdateInvariantText(nest.InnerKey, "an unpublished draft edit");

        await ReindexService.WaitForPendingReindexAsync();
        Assert.IsFalse(_indexed.ContainsKey(nest.Document.Id), "A draft save changes no published value, so the document must not be reindexed.");
    }

    [Test]
    public async Task Unpublishing_Inner_Removes_Inner_Content_But_Keeps_Outer()
    {
        var nest = await CreateNestedStructure();

        await ResetTracking();
        await UnpublishElement(nest.InnerKey);

        var blocks = GetDocumentBlocks(nest.Document.Id);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(blocks.Contains(InnerText), "Unpublishing the inner element should drop its content from the document.");
            Assert.IsTrue(blocks.Contains(OuterText), "The outer element's content should remain indexed.");
        });
    }

    [Test]
    public async Task Unpublishing_Outer_Removes_Both_Outer_And_Inner_Content()
    {
        var nest = await CreateNestedStructure();

        await ResetTracking();
        await UnpublishElement(nest.OuterKey);

        var blocks = GetDocumentBlocks(nest.Document.Id);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(blocks.Contains(OuterText), "Unpublishing the outer element should drop its content from the document.");
            Assert.IsFalse(blocks.Contains(InnerText), "The inner content, reachable only through the outer element, should also be dropped.");
        });
    }

    [Test]
    public async Task Trashing_Inner_Removes_Inner_Content_But_Keeps_Outer()
    {
        var nest = await CreateNestedStructure();

        await ResetTracking();
        Assert.IsTrue((await ElementEditingService.MoveToRecycleBinAsync(nest.InnerKey, Constants.Security.SuperUserKey)).Success);

        var blocks = GetDocumentBlocks(nest.Document.Id);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(blocks.Contains(InnerText), "Trashing the inner element should drop its content from the document.");
            Assert.IsTrue(blocks.Contains(OuterText), "The outer element's content should remain indexed.");
        });
    }

    [Test]
    public async Task Deleting_Trashed_Inner_Keeps_Its_Content_Out_Of_The_Document()
    {
        // Trashing already removes the inner content (and reindexes the document). Permanently deleting the trashed
        // element is a clean no-op: it does not reindex again (the relation is gone with the node), and it must not
        // resurface the content. We keep the document's value set from the trash reindex and confirm it stays clean.
        var nest = await CreateNestedStructure();

        await ResetTracking();
        Assert.IsTrue((await ElementEditingService.MoveToRecycleBinAsync(nest.InnerKey, Constants.Security.SuperUserKey)).Success);
        await ReindexService.WaitForPendingReindexAsync();
        Assert.IsTrue((await ElementEditingService.DeleteFromRecycleBinAsync(nest.InnerKey, Constants.Security.SuperUserKey)).Success);

        var blocks = GetDocumentBlocks(nest.Document.Id);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(blocks.Contains(InnerText), "The trashed-then-deleted inner content must stay out of the document.");
            Assert.IsTrue(blocks.Contains(OuterText), "The outer element's content should remain indexed.");
        });
    }

    [Test]
    public async Task Restoring_Inner_Reindexes_Document_But_Content_Stays_Absent()
    {
        // Restore unpublishes a previously-published trashed element, so its content is absent both before (trashed)
        // and after (unpublished) the restore. The document is still reindexed (restore is a branch refresh), but the
        // inner content stays out until the element is published again. Reading the value set also confirms the reindex.
        var nest = await CreateNestedStructure();
        Assert.IsTrue((await ElementEditingService.MoveToRecycleBinAsync(nest.InnerKey, Constants.Security.SuperUserKey)).Success);

        await ResetTracking();
        Assert.IsTrue((await ElementEditingService.RestoreAsync(nest.InnerKey, null, Constants.Security.SuperUserKey)).Success);

        var blocks = GetDocumentBlocks(nest.Document.Id);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(blocks.Contains(InnerText), "A restored element comes back unpublished, so its content stays out of the document.");
            Assert.IsTrue(blocks.Contains(OuterText), "The outer element's content should remain indexed.");
        });
    }

    private async Task<NestedStructure> CreateNestedStructure()
    {
        var innerElementType = await CreateElementType(ContentVariation.Nothing, "myInnerElementType");
        var innerBlockList = await CreateBlockListDataType(innerElementType);
        var outerElementType = await CreateElementTypeWithTextAndBlocks(innerBlockList, "myOuterElementType", "My Outer Element Type");
        var outerBlockList = await CreateBlockListDataType(outerElementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, outerBlockList);

        var innerKey = await CreateAndPublishInnerElement(innerElementType.Key);
        var outerKey = await CreateAndPublishOuterElement(outerElementType.Key, innerKey);
        var document = CreateDocumentEmbeddingElement(contentType, outerKey);

        return new NestedStructure(innerKey, outerKey, document);
    }

    private async Task<string> ReindexAndGetDocumentBlocks(NestedStructure nest)
    {
        await ResetTracking();
        await PublishElement(nest.InnerKey);
        return GetDocumentBlocks(nest.Document.Id);
    }

    private string GetDocumentBlocks(int documentId)
    {
        ReindexService.WaitForPendingReindexAsync().GetAwaiter().GetResult();
        Assert.IsTrue(_indexed.TryGetValue(documentId, out ValueSet? valueSet), $"Expected document {documentId} to have been reindexed.");
        return valueSet!.Values.TryGetValue("blocks", out var values)
            ? string.Join(Environment.NewLine, values.Select(v => v?.ToString()))
            : string.Empty;
    }

    private async Task ResetTracking()
    {
        await ReindexService.WaitForPendingReindexAsync();
        _indexed.Clear();
    }

    private async Task<Guid> CreateAndPublishInnerElement(Guid elementTypeKey)
        => await CreateAndPublishElement(
            elementTypeKey,
            [new PropertyValueModel { Alias = "invariantText", Value = InnerText }]);

    private async Task<Guid> CreateAndPublishOuterElement(Guid elementTypeKey, Guid innerElementKey)
        => await CreateAndPublishElement(
            elementTypeKey,
            [
                new PropertyValueModel { Alias = "invariantText", Value = OuterText },
                new PropertyValueModel { Alias = "blocks", Value = SerializeExternalBlock(innerElementKey) },
            ]);

    private async Task<Guid> CreateAndPublishElement(Guid elementTypeKey, PropertyValueModel[] properties)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties = properties,
                Variants = [new VariantModel { Name = "Element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;
        await PublishElement(elementKey);
        return elementKey;
    }

    private async Task PublishElement(Guid elementKey)
    {
        var result = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
    }

    private async Task UnpublishElement(Guid elementKey)
        => Assert.IsTrue(
            (await ElementPublishingService.UnpublishAsync(elementKey, null, Constants.Security.SuperUserKey)).Success);

    private async Task UpdateInvariantText(Guid elementKey, string text)
    {
        var result = await ElementEditingService.UpdateAsync(
            elementKey,
            new ElementUpdateModel
            {
                Properties = [new PropertyValueModel { Alias = "invariantText", Value = text }],
                Variants = [new VariantModel { Name = "Element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
    }

    private IContent CreateDocumentEmbeddingElement(IContentType contentType, Guid sharedElementKey)
    {
        var content = new ContentBuilder().WithContentType(contentType).WithName("Page").Build();
        content.Properties["blocks"]!.SetValue(SerializeExternalBlock(sharedElementKey));
        ContentService.Save(content);
        PublishContent(content, ["*"]);
        return content;
    }

    private string SerializeExternalBlock(Guid elementKey)
        => JsonSerializer.Serialize(
            new BlockListValue
            {
                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                {
                    {
                        Constants.PropertyEditors.Aliases.BlockList,
                        [new BlockListLayoutItem { ContentKey = elementKey, IsExternalContent = true }]
                    },
                },
                ContentData = [],
                SettingsData = [],
                Expose = [],
            });

    private async Task<IContentType> CreateElementTypeWithTextAndBlocks(IDataType blocksDataType, string alias, string name)
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName(name)
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blocksDataType.Id)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key },
            });

    private sealed record NestedStructure(Guid InnerKey, Guid OuterKey, IContent Document);

    private sealed class CapturingIndexingHandler : IUmbracoIndexingHandler
    {
        private readonly IPublishedContentValueSetBuilder _publishedValueSetBuilder;
        private readonly ConcurrentDictionary<int, ValueSet> _indexed;

        public CapturingIndexingHandler(
            IPublishedContentValueSetBuilder publishedValueSetBuilder,
            ConcurrentDictionary<int, ValueSet> indexed)
        {
            _publishedValueSetBuilder = publishedValueSetBuilder;
            _indexed = indexed;
        }

        public bool Enabled => true;

        public void ReIndexForContent(IContent sender, bool isPublished)
        {
            if (isPublished)
            {
                _indexed[sender.Id] = _publishedValueSetBuilder.GetValueSets(sender).Single();
            }
            else
            {
                _indexed.TryRemove(sender.Id, out _);
            }
        }

        public void ReIndexForMedia(IMedia sender, bool isPublished)
        {
        }

        public void ReIndexForMember(IMember member)
        {
        }

        public void RemoveProtectedContent()
        {
        }

        public void DeleteDocumentsForContentTypes(IReadOnlyCollection<int> removedContentTypes)
        {
        }

        public void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
        {
        }

        public void DeleteIndexForEntities(IReadOnlyCollection<int> entityIds, bool keepIfUnpublished)
        {
        }
    }
}
