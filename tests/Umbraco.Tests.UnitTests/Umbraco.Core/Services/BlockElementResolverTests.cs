// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class BlockElementResolverTests
{
    [Test]
    public void The_Block_Is_Found_In_Whichever_Stored_Slot_Holds_It()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyOf("blocks", Slot("da-dk", null, "da-value"), Slot("en-us", null, "en-value"));

        BlockItemData? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey, "en-value"), EmptyPropertyEditors(), blockKey);

        Assert.IsNotNull(result);
        Assert.AreEqual(blockKey, result.Key);
    }

    [Test]
    public void No_Slot_Holding_The_Block_Yields_Nothing()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyOf("blocks", Slot(null, null, "value"));

        BlockItemData? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey), EmptyPropertyEditors(), blockKey);

        Assert.IsNull(result);
    }

    [Test]
    public void The_Block_Is_Found_In_Whichever_Property_Holds_It()
    {
        Guid blockKey = Guid.NewGuid();
        IDataValueEditor blockEditor = BlockValueEditorFinding(blockKey, "second-value");

        BlockItemData? result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [
                PropertyOf("blocks", Slot(null, null, "first-value")),
                PropertyOf("blocks", Slot(null, null, "second-value")),
            ],
            PropertyEditors(("blocks", blockEditor)),
            blockKey);

        Assert.IsNotNull(result);
        Assert.AreEqual(blockKey, result.Key);
    }

    [Test]
    public void Properties_That_Are_Not_Block_Editors_Are_Passed_Over()
    {
        Guid blockKey = Guid.NewGuid();
        IDataValueEditor blockEditor = BlockValueEditorFinding(blockKey, "value");

        BlockItemData? result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [
                PropertyOf("textbox", Slot(null, null, "value")),
                PropertyOf("blocks", Slot(null, null, "value")),
            ],
            PropertyEditors(("blocks", blockEditor), ("textbox", Mock.Of<IDataValueEditor>())),
            blockKey);

        Assert.IsNotNull(result);
        Assert.AreEqual(blockKey, result.Key);
    }

    [Test]
    public void No_Property_Holding_The_Block_Yields_Nothing()
    {
        Guid blockKey = Guid.NewGuid();

        BlockItemData? result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [PropertyOf("blocks", Slot(null, null, "value"))],
            PropertyEditors(("blocks", BlockValueEditorFinding(blockKey))),
            blockKey);

        Assert.IsNull(result);
    }

    [Test]
    public void A_Block_Nested_Inside_Another_Block_Is_Found()
    {
        // a block's own properties can be block editors in their own right, so the search has to descend
        // through them - the nested block is not in the outer block value's own content data.
        Guid nestedKey = Guid.NewGuid();
        var nestedValue = BlockValueHolding(nestedKey);

        BlockItemData outerBlock = new(Guid.NewGuid(), Guid.NewGuid(), "outer");
        outerBlock.Values.Add(new BlockPropertyValue
        {
            Alias = "inner",
            Value = "nested-raw",
            PropertyType = Mock.Of<IPropertyType>(type => type.PropertyEditorAlias == "blocks"),
        });

        var outerValue = new BlockListValue();
        outerValue.ContentData.Add(outerBlock);

        IDataValueEditor blockEditor = BlockValueEditorReturning(("outer-raw", outerValue), ("nested-raw", nestedValue));

        BlockItemData? result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [PropertyOf("blocks", Slot(null, null, "outer-raw"))],
            PropertyEditors(("blocks", blockEditor)),
            nestedKey);

        Assert.IsNotNull(result);
        Assert.AreEqual(nestedKey, result.Key);
    }

    private static PropertyEditorCollection EmptyPropertyEditors()
        => new(new DataEditorCollection(Enumerable.Empty<IDataEditor>));

    private static IProperty PropertyOf(string editorAlias, params IPropertyValue[] values)
    {
        IPropertyType propertyType = Mock.Of<IPropertyType>(type => type.PropertyEditorAlias == editorAlias);
        return Mock.Of<IProperty>(property => property.Values == values && property.PropertyType == propertyType);
    }

    private static IPropertyValue Slot(string? culture, string? segment, object? editedValue)
        => Mock.Of<IPropertyValue>(value =>
            value.Culture == culture &&
            value.Segment == segment &&
            value.EditedValue == editedValue);

    private static PropertyEditorCollection PropertyEditors(params (string Alias, IDataValueEditor ValueEditor)[] editors)
    {
        IDataEditor[] dataEditors = editors
            .Select(editor => Mock.Of<IDataEditor>(x => x.Alias == editor.Alias && x.GetValueEditor() == editor.ValueEditor))
            .ToArray();

        return new PropertyEditorCollection(new DataEditorCollection(() => dataEditors));
    }

    /// <summary>
    ///     A block value editor whose stored values hold the block only where named, and nowhere else.
    /// </summary>
    private static IBlockValueEditor EditorFinding(Guid blockKey, params object[] valuesHoldingTheBlock)
    {
        var editor = new Mock<IBlockValueEditor>();
        editor
            .Setup(x => x.GetBlockValue(It.IsAny<object?>()))
            .Returns((object? value) => value is not null && valuesHoldingTheBlock.Contains(value)
                ? BlockValueHolding(blockKey)
                : null);

        return editor.Object;
    }

    /// <summary>
    ///     The same, as a value editor a data editor can hand out.
    /// </summary>
    private static IDataValueEditor BlockValueEditorFinding(Guid blockKey, params object[] valuesHoldingTheBlock)
        => BlockValueEditorReturning(valuesHoldingTheBlock.Select(value => (value, BlockValueHolding(blockKey))).ToArray());

    /// <summary>
    ///     A value editor mapping each named stored value to the block value it deserializes into.
    /// </summary>
    private static IDataValueEditor BlockValueEditorReturning(params (object Stored, BlockValue Parsed)[] values)
    {
        var editor = new Mock<IDataValueEditor>();
        editor
            .As<IBlockValueEditor>()
            .Setup(x => x.GetBlockValue(It.IsAny<object?>()))
            .Returns((object? value) => values.FirstOrDefault(x => Equals(x.Stored, value)).Parsed);

        return editor.Object;
    }

    private static BlockValue BlockValueHolding(Guid blockKey)
    {
        var blockValue = new BlockListValue();
        blockValue.ContentData.Add(new BlockItemData(blockKey, Guid.NewGuid(), "block"));
        return blockValue;
    }
}
