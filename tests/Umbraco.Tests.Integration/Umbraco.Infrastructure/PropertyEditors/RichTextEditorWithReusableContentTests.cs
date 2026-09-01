using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

// NOTE: These tests are in place to ensure that elements based on reusable content work for Rich Text. The feature
//       is tested more in-depth for Block List (see BlockListWithReusableContentTest), and since the actual
//       implementation is shared between Block List and Rich Text, we won't repeat all those tests here.
internal class RichTextEditorWithReusableContentTests : BlockEditorWithReusableContentTestBase
{
    [Test]
    public async Task Can_Handle_Reusable_Element()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var richTextDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, richTextDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var richTextValue = new RichTextEditorValue
        {
            Markup = $"""
                      <p>Some text.</p>
                      <umb-rte-block data-content-key="{reusableElementKey:D}"><!--Umbraco-Block--></umb-rte-block>
                      <p>More text.</p>
                      """,
            Blocks = new RichTextBlockValue
            {
                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                {
                    {
                        Constants.PropertyEditors.Aliases.RichText, [
                            new RichTextBlockLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
                        ]
                    },
                },
                ContentData = [],
                SettingsData = [],
                Expose = [],
            },
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(richTextValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var property = publishedContent.GetProperty("blocks");
        Assert.That(property, Is.Not.Null);

        var propertyValue = property.GetDeliveryApiValue(false, "en-US") as RichTextModel;
        Assert.That(propertyValue, Is.Not.Null);

        var blocks = propertyValue.Blocks.ToArray();
        Assert.That(blocks, Has.Length.EqualTo(1));

        var block = blocks.First();
        Assert.That(block.Content.Id, Is.EqualTo(reusableElementKey));
        Assert.That(block.Content.Properties, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(block.Content.Properties["invariantText"], Is.EqualTo("The reusable invariant text"));
            Assert.That(block.Content.Properties["variantText"], Is.EqualTo("The reusable variant text"));
        });
    }

    private async Task<IDataType> CreateRichTextDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.RichText,
            new RichTextConfiguration.RichTextBlockConfiguration[]
            {
                new()
                {
                    ContentElementTypeKey = elementType.Key,
                    SettingsElementTypeKey = elementType.Key,
                }
            });
}
