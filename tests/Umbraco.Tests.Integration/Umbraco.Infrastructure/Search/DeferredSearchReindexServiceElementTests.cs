using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;
using IUmbracoBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Search;

internal sealed class DeferredSearchReindexServiceElementTests : BlockEditorWithReusableContentTestBase
{
    private DeferredSearchReindexService Service
        => (DeferredSearchReindexService)GetRequiredService<IDeferredSearchReindexService>();

    private int ElementId(Guid key) => IdKeyMap.GetIdForKey(key, UmbracoObjectTypes.Element).Result;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>();
    }

    [Test]
    public async Task Finds_Document_Directly_Referencing_Element()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var elementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);
        var content = CreateDocumentEmbeddingElement(contentType, elementKey);

        var elementId = ElementId(elementKey);
        var documentIds = Service.FindDocumentIdsReferencingElements([elementId]);

        Assert.Contains(content.Id, documentIds.ToArray());
    }

    [Test]
    public async Task Does_Not_Find_Document_Referencing_Element_Via_Generic_Element_Relation()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        // A published element, and a plain document with NO external block content.
        var elementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);
        var document = new ContentBuilder().WithContentType(contentType).WithName("Picker page").Build();
        ContentService.Save(document);
        PublishContent(document, ["*"]);

        var elementId = ElementId(elementKey);

        // Simulate an element-picker reference: a generic umbElement relation document(parent) -> element(child).
        var relationService = GetRequiredService<IRelationService>();
        relationService.Relate(document.Id, elementId, Constants.Conventions.RelationTypes.RelatedElementAlias);

        var documentIds = Service.FindDocumentIdsReferencingElements([elementId]);

        Assert.IsFalse(documentIds.Contains(document.Id), "Documents referencing an element only via the generic umbElement relation (element picker) must not be reindexed.");
    }

    private IContent CreateDocumentEmbeddingElement(IContentType contentType, Guid sharedElementKey)
    {
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = sharedElementKey, IsExternalContent = true }]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var content = new ContentBuilder().WithContentType(contentType).WithName("Page").Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, ["*"]);
        return content;
    }

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
            });
}
