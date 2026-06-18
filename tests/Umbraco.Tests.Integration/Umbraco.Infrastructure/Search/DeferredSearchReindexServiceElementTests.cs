using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Search;

internal sealed class DeferredSearchReindexServiceElementTests : BlockEditorWithReusableContentTestBase
{
    private DeferredSearchReindexService Service
        => (DeferredSearchReindexService)GetRequiredService<IDeferredSearchReindexService>();

    private IRelationService RelationService => GetRequiredService<IRelationService>();

    private int ElementId(Guid key) => IdKeyMap.GetIdForKey(key, UmbracoObjectTypes.Element).Result;

    [Test]
    public async Task Finds_Document_Directly_Referencing_Element()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var elementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);
        var content = CreateDocumentEmbeddingElement(contentType, elementKey);

        var elementId = ElementId(elementKey);
        CreateElementRelation(content.Id, elementId);

        var documentIds = Service.FindDocumentIdsReferencingElements([elementId]);

        Assert.Contains(content.Id, documentIds.ToArray());
    }

    private void CreateElementRelation(int documentId, int elementId)
    {
        IRelationType? relationType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedElementAlias);
        Assert.IsNotNull(relationType, "The umbElement relation type must exist.");
        RelationService.Relate(documentId, elementId, relationType);
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
