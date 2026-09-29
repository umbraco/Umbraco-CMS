// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

#pragma warning disable CS0618 // Type or member is obsolete

[TestFixture]
internal sealed class SingleBlockConversionVerifierTests
{
    [Test]
    public void Can_Count_No_Single_Block_Values_In_Unconverted_Value()
    {
        BlockListValue blockListValue = BuildBlockListValue(BuildBlockItemData("text", "some text"));

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockValues(blockListValue), Is.Zero);
    }

    [Test]
    public void Can_Count_Single_Block_Value_At_Top_Level()
    {
        var singleBlockValue = new SingleBlockValue(new SingleBlockLayoutItem { ContentKey = Guid.NewGuid() });

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockValues(singleBlockValue), Is.EqualTo(1));
    }

    [Test]
    public void Can_Count_Nested_Single_Block_Values()
    {
        // A Block List holding two blocks, each with a converted single block value, one of which holds another.
        BlockListValue blockListValue = BuildBlockListValue(
            BuildBlockItemData("items", BuildSingleBlockValue()),
            BuildBlockItemData("items", BuildSingleBlockValue(BuildBlockItemData("items", BuildSingleBlockValue()))));

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockValues(blockListValue), Is.EqualTo(3));
    }

    [Test]
    public void Can_Count_Single_Block_Values_Nested_In_Rich_Text()
    {
        var richTextEditorValue = new RichTextEditorValue
        {
            Markup = "<p>Some markup</p>",
            Blocks = new RichTextBlockValue
            {
                ContentData = [BuildBlockItemData("items", BuildSingleBlockValue())],
            },
        };

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockValues(richTextEditorValue), Is.EqualTo(1));
    }

    [TestCase(null)]
    [TestCase("some text")]
    [TestCase("")]
    public void Can_Count_No_Single_Block_Values_In_Non_Block_Value(string? value)
        => Assert.That(SingleBlockConversionVerifier.CountSingleBlockValues(value), Is.Zero);

    [Test]
    public void Can_Count_Single_Block_Layout_At_Top_Level()
    {
        var json = $@"{{""contentData"":[],""layout"":{{""{Constants.PropertyEditors.Aliases.SingleBlock}"":[{{""contentKey"":""{Guid.NewGuid()}""}}]}}}}";

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockLayouts(json), Is.EqualTo(1));
    }

    [Test]
    public void Can_Count_No_Single_Block_Layout_In_Block_List_Value()
    {
        var json = $@"{{""contentData"":[],""layout"":{{""{Constants.PropertyEditors.Aliases.BlockList}"":[{{""contentKey"":""{Guid.NewGuid()}""}}]}}}}";

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockLayouts(json), Is.Zero);
    }

    [Test]
    public void Can_Count_Single_Block_Layout_Nested_In_A_Serialized_Value()
    {
        // Nested block editor values are stored as JSON strings within their parent.
        var nestedJson = $@"{{""layout"":{{""{Constants.PropertyEditors.Aliases.SingleBlock}"":[{{""contentKey"":""{Guid.NewGuid()}""}}]}}}}";
        var json = $@"{{""contentData"":[{{""values"":[{{""alias"":""items"",""value"":{ToJsonString(nestedJson)}}}]}}],""layout"":{{""{Constants.PropertyEditors.Aliases.BlockList}"":[{{""contentKey"":""{Guid.NewGuid()}""}}]}}}}";

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockLayouts(json), Is.EqualTo(1));
    }

    [Test]
    public void Can_Count_Single_Block_Layout_Stored_With_A_Pascal_Cased_Property_Name()
    {
        var json = $@"{{""ContentData"":[],""Layout"":{{""{Constants.PropertyEditors.Aliases.SingleBlock}"":[{{""contentKey"":""{Guid.NewGuid()}""}}]}}}}";

        Assert.That(SingleBlockConversionVerifier.CountSingleBlockLayouts(json), Is.EqualTo(1));
    }

    [Test]
    public void Can_Count_No_Single_Block_Layout_In_Invalid_Json()
        => Assert.That(SingleBlockConversionVerifier.CountSingleBlockLayouts("not json at all"), Is.Zero);

    private static string ToJsonString(string value) => System.Text.Json.JsonSerializer.Serialize(value);

    private static SingleBlockValue BuildSingleBlockValue(params BlockItemData[] contentData)
        => new(new SingleBlockLayoutItem { ContentKey = Guid.NewGuid() }) { ContentData = [.. contentData] };

    private static BlockListValue BuildBlockListValue(params BlockItemData[] contentData)
        => new() { ContentData = [.. contentData] };

    private static BlockItemData BuildBlockItemData(string propertyAlias, object? propertyValue)
        => new()
        {
            Key = Guid.NewGuid(),
            ContentTypeKey = Guid.NewGuid(),
            Values = [new BlockPropertyValue { Alias = propertyAlias, Value = propertyValue }],
        };
}
