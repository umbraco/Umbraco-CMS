using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

/// <summary>
/// Tests that the published values of the Block List and Single Block property editors are created from values stored
/// by either of the two editors, so a data type can be switched between them without converting its stored values.
/// </summary>
internal sealed class BlockEditorInterchangeabilityTests : BlockEditorElementVariationTestBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    [Test]
    public async Task Single_Block_Publishes_Value_Stored_By_Block_List()
    {
        IContentType elementType = await CreateElementType(ContentVariation.Nothing);
        IContentType contentType = await CreateContentType(ContentVariation.Nothing, await CreateDataType(Constants.PropertyEditors.Aliases.SingleBlock, elementType));

        var contentKey = Guid.NewGuid();
        var storedValue = new BlockListValue([new BlockListLayoutItem(contentKey)])
        {
            ContentData = [CreateBlockItemData(elementType, contentKey, "The block")],
            Expose = [new BlockItemVariation(contentKey, null, null)],
        };

        IContent content = CreateAndPublishContent(contentType, storedValue);

        BlockListItem? publishedValue = GetPublishedContent(content.Key).Value<BlockListItem>("blocks");

        Assert.IsNotNull(publishedValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentKey, publishedValue!.Content.Key);
            Assert.AreEqual("The block", publishedValue.Content.Value<string>("invariantText"));
        });
    }

    [Test]
    public async Task Single_Block_Publishes_First_Block_Of_Value_Holding_Multiple_Blocks()
    {
        IContentType elementType = await CreateElementType(ContentVariation.Nothing);
        IDataType dataType = await CreateDataType(Constants.PropertyEditors.Aliases.BlockList, elementType);
        IContentType contentType = await CreateContentType(ContentVariation.Nothing, dataType);

        Guid[] contentKeys = [Guid.NewGuid(), Guid.NewGuid()];
        var storedValue = new BlockListValue(contentKeys.Select(key => new BlockListLayoutItem(key)))
        {
            ContentData = contentKeys.Select((key, index) => CreateBlockItemData(elementType, key, $"Block {index + 1}")).ToList(),
            Expose = contentKeys.Select(key => new BlockItemVariation(key, null, null)).ToList(),
        };

        // a value holding multiple blocks fails single block validation, so it can only be published as a single
        // block value by switching the data type over after the value was published by the block list
        IContent content = CreateAndPublishContent(contentType, storedValue);
        await SwitchPropertyEditorAsync(dataType, Constants.PropertyEditors.Aliases.SingleBlock, contentType);

        BlockListItem? publishedValue = GetPublishedContent(content.Key).Value<BlockListItem>("blocks");

        Assert.IsNotNull(publishedValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentKeys[0], publishedValue!.Content.Key);
            Assert.AreEqual("Block 1", publishedValue.Content.Value<string>("invariantText"));
        });
    }

    [Test]
    public async Task Block_List_Publishes_Value_Stored_By_Single_Block()
    {
        IContentType elementType = await CreateElementType(ContentVariation.Nothing);
        IContentType contentType = await CreateContentType(ContentVariation.Nothing, await CreateDataType(Constants.PropertyEditors.Aliases.BlockList, elementType));

        var contentKey = Guid.NewGuid();
        var storedValue = new SingleBlockValue(new SingleBlockLayoutItem { ContentKey = contentKey })
        {
            ContentData = [CreateBlockItemData(elementType, contentKey, "The block")],
            Expose = [new BlockItemVariation(contentKey, null, null)],
        };

        IContent content = CreateAndPublishContent(contentType, storedValue);

        BlockListModel? publishedValue = GetPublishedContent(content.Key).Value<BlockListModel>("blocks");

        Assert.IsNotNull(publishedValue);
        Assert.AreEqual(1, publishedValue!.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentKey, publishedValue[0].Content.Key);
            Assert.AreEqual("The block", publishedValue[0].Content.Value<string>("invariantText"));
        });
    }

    private async Task<IDataType> CreateDataType(string propertyEditorAlias, IContentType elementType)
        => await CreateBlockEditorDataType(
            propertyEditorAlias,
            new BlockListConfiguration.BlockConfiguration[] { new() { ContentElementTypeKey = elementType.Key } });

    private async Task SwitchPropertyEditorAsync(IDataType dataType, string propertyEditorAlias, IContentType contentType)
    {
        dataType.Editor = PropertyEditorCollection[propertyEditorAlias];
        Attempt<IDataType, DataTypeOperationStatus> result = await DataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        RefreshContentTypeCache(contentType);
    }

    private static BlockItemData CreateBlockItemData(IContentType elementType, Guid key, string invariantText)
        => new()
        {
            Key = key,
            ContentTypeAlias = elementType.Alias,
            ContentTypeKey = elementType.Key,
            Values = [new BlockPropertyValue { Alias = "invariantText", Value = invariantText }],
        };

    private IContent CreateAndPublishContent(IContentType contentType, BlockValue storedValue)
    {
        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = JsonSerializer.Serialize(storedValue) })
            .Build();
        ContentService.Save(content);
        PublishContent(content, ["*"]);

        return content;
    }
}
