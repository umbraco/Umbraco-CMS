using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

/// <summary>
/// Regression test for https://github.com/umbraco/Umbraco-CMS/issues/22306
/// BlockGrid throws "Nullable object must have a value" when a block item has null columnSpan/rowSpan.
/// </summary>
internal sealed class BlockGridNullSpanTests : BlockEditorElementVariationTestBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [Test]
    public async Task Can_Render_BlockGrid_When_Items_Have_Null_Spans()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var areaKey = Guid.NewGuid();

        var blockGridDataType = await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockGrid,
            new BlockGridConfiguration.BlockGridBlockConfiguration[]
            {
                new()
                {
                    ContentElementTypeKey = elementType.Key,
                    AreaGridColumns = 12,
                    Areas = [new() { Alias = "one", Key = areaKey, ColumnSpan = 12, RowSpan = 1 }],
                },
            });

        var contentType = CreateContentType(ContentVariation.Nothing, blockGridDataType);

        var rootKey = Guid.NewGuid();
        var nestedKey = Guid.NewGuid();
        var blockGridValue = new BlockGridValue(
        [
            new BlockGridLayoutItem(rootKey)
            {
                // Root item also has null spans to cover both code paths.
                ColumnSpan = null,
                RowSpan = null,
                Areas =
                [
                    new BlockGridLayoutAreaItem(areaKey)
                    {
                        Items =
                        [
                            // Nested item with null spans — the scenario from issue #22306.
                            new BlockGridLayoutItem(nestedKey) { ColumnSpan = null, RowSpan = null },
                        ],
                    },
                ],
            },
        ])
        {
            ContentData =
            [
                new(rootKey, elementType.Key, elementType.Alias) { Values = [new() { Alias = "invariantText", Value = "Root" }] },
                new(nestedKey, elementType.Key, elementType.Alias) { Values = [new() { Alias = "invariantText", Value = "Nested" }] },
            ],
            Expose = [new(rootKey, null, null), new(nestedKey, null, null)],
        };

        var content = new ContentBuilder().WithContentType(contentType).WithName("Home").Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockGridValue));
        ContentService.Save(content);
        PublishContent(content, ["*"]);

        SetVariationContext(null, null);
        var published = GetPublishedContent(content.Key);

        // Before the fix this threw InvalidOperationException: "Nullable object must have a value".
        var blocks = published.Value<BlockGridModel>("blocks");

        Assert.IsNotNull(blocks);
        Assert.AreEqual(1, blocks!.Count);

        // Null RowSpan defaults to 1, null ColumnSpan defaults to gridColumns (12).
        Assert.AreEqual(1, blocks[0].RowSpan);
        Assert.AreEqual(12, blocks[0].ColumnSpan);

        var area = blocks[0].Areas.FirstOrDefault();
        Assert.IsNotNull(area);
        Assert.AreEqual(1, area!.Count);

        // Nested item gets the same defaults.
        Assert.AreEqual(1, area[0].RowSpan);
        Assert.AreEqual(12, area[0].ColumnSpan);
    }
}
