using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
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
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementPublishedNotification, ContentRelationsUpdate>();
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
    public async Task Does_Not_Find_Document_Referencing_Element_Via_Element_Picker()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var elementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        // A document that references the element through an element picker, which produces a generic umbElement relation.
        var elementPickerDataType = await CreateElementPickerDataType();
        var contentType = await CreateContentType(ContentVariation.Nothing, elementPickerDataType);
        var document = new ContentBuilder().WithContentType(contentType).WithName("Picker page").Build();
        document.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(new[] { elementKey }));
        ContentService.Save(document);
        PublishContent(document, ["*"]);

        var elementId = ElementId(elementKey);

        // The picker created a umbElement relation from the document to the element...
        var relationService = GetRequiredService<IRelationService>();
        var pickerRelations = relationService.GetByParent(document, Constants.Conventions.RelationTypes.RelatedElementAlias).ToArray();
        Assert.AreEqual(1, pickerRelations.Length, "The element picker should create a umbElement relation to the element.");

        // ...but a picker reference does not index the element's content, so the document must not be reindexed.
        var documentIds = Service.FindDocumentIdsReferencingElements([elementId]);
        Assert.IsFalse(documentIds.Contains(document.Id), "Documents referencing an element only via the element picker must not be reindexed.");
    }

    private async Task<IDataType> CreateElementPickerDataType()
    {
        var dataType = new DataType(
            GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.ElementPicker],
            GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            Name = "My Element Picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await GetRequiredService<IDataTypeService>().CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    [Test]
    public async Task Finds_Document_Referencing_Element_Through_Nested_Element()
    {
        // Inner element type holds plain text; outer element type holds a block-list of inner elements.
        var innerElementType = await CreateElementType(ContentVariation.Nothing, "myInnerElementType");
        var innerBlockListDataType = await CreateBlockListDataType(innerElementType);
        var outerElementType = await CreateElementTypeWithBlocks(innerBlockListDataType, "myOuterElementType", "My Outer Element Type");
        var outerBlockListDataType = await CreateBlockListDataType(outerElementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, outerBlockListDataType);

        // Inner reusable element B.
        var innerElementKey = await CreateAndPublishInvariantReusableElement(innerElementType.Key);

        // Outer reusable element A that embeds B as external block content.
        var outerElementKey = await CreateAndPublishReusableElementEmbeddingElement(outerElementType.Key, innerElementKey);

        // Document D that embeds A as external block content.
        var document = CreateDocumentEmbeddingElement(contentType, outerElementKey);

        var innerElementId = ElementId(innerElementKey);
        var documentIds = Service.FindDocumentIdsReferencingElements([innerElementId]);

        Assert.Contains(document.Id, documentIds.ToArray(), "Editing the inner element must reindex the document that transitively embeds it.");
    }

    private async Task<IContentType> CreateElementTypeWithBlocks(IDataType blocksEditorDataType, string alias, string name)
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName(name)
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blocksEditorDataType.Id)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<Guid> CreateAndPublishReusableElementEmbeddingElement(Guid elementTypeKey, Guid embeddedElementKey)
    {
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = embeddedElementKey, IsExternalContent = true }]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) },
                ],
                Variants =
                [
                    new VariantModel { Name = "Outer reusable element" }
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        return elementKey;
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
