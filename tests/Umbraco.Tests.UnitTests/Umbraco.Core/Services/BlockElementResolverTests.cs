// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class BlockElementResolverTests
{
    [Test]
    public void Invariant_Property_Is_Live_In_Every_Culture_The_Block_Is_Exposed_In()
    {
        IContentType blockType = VariantBlockType();
        Guid blockKey = Guid.NewGuid();

        var result = BlockElementResolver.ResolveLiveCultures(
            [Segment(blockKey, blockType, "da-dk", "en-us", "da-dk")],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(blockType));

        CollectionAssert.AreEquivalent(new[] { "da-dk", "en-us" }, result);
    }

    [Test]
    public void Invariant_Property_With_An_Invariant_Block_Is_Live_In_A_Single_Null_Culture()
    {
        IContentType blockType = InvariantBlockType();
        Guid blockKey = Guid.NewGuid();

        var result = BlockElementResolver.ResolveLiveCultures(
            [Segment(blockKey, blockType, [null])],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(blockType));

        CollectionAssert.AreEqual(new string?[] { null }, result);
    }

    [Test]
    public void A_Block_Exposed_In_No_Culture_Is_Live_In_No_Culture()
    {
        IContentType blockType = VariantBlockType();

        var result = BlockElementResolver.ResolveLiveCultures(
            [Segment(Guid.NewGuid(), blockType)],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(blockType));

        Assert.IsEmpty(result);
    }

    [Test]
    public void Variant_Property_Is_Live_Only_In_Its_Own_Culture_Regardless_Of_Exposures()
    {
        IContentType blockType = VariantBlockType();
        Guid blockKey = Guid.NewGuid();

        var result = BlockElementResolver.ResolveLiveCultures(
            [Segment(blockKey, blockType, "da-dk", "en-us")],
            ContentVariation.Culture,
            propertyVariesByCulture: true,
            propertyCulture: "da-dk",
            propertySegment: null,
            ContentTypes(blockType));

        CollectionAssert.AreEqual(new[] { "da-dk" }, result);
    }

    [Test]
    public void Variant_Property_With_Nothing_Exposed_Is_Live_In_No_Culture()
    {
        IContentType blockType = VariantBlockType();

        var result = BlockElementResolver.ResolveLiveCultures(
            [Segment(Guid.NewGuid(), blockType)],
            ContentVariation.Culture,
            propertyVariesByCulture: true,
            propertyCulture: "da-dk",
            propertySegment: null,
            ContentTypes(blockType));

        Assert.IsEmpty(result);
    }

    [Test]
    public void A_Nested_Block_Is_Live_Only_In_Cultures_Every_Block_Around_It_Is_Exposed_In()
    {
        IContentType outerType = VariantBlockType();
        IContentType innerType = VariantBlockType();

        var result = BlockElementResolver.ResolveLiveCultures(
            [
                Segment(Guid.NewGuid(), outerType, "da-dk", "en-us"),
                Segment(Guid.NewGuid(), innerType, "da-dk"),
            ],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(outerType, innerType));

        CollectionAssert.AreEqual(new[] { "da-dk" }, result);
    }

    [Test]
    public void A_Nested_Block_Is_Live_In_No_Culture_When_The_Block_Around_It_Is_Not_Exposed()
    {
        IContentType outerType = VariantBlockType();
        IContentType innerType = VariantBlockType();

        var result = BlockElementResolver.ResolveLiveCultures(
            [
                Segment(Guid.NewGuid(), outerType),
                Segment(Guid.NewGuid(), innerType, "da-dk"),
            ],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(outerType, innerType));

        Assert.IsEmpty(result);
    }

    [Test]
    public void A_Block_Inside_An_Invariant_Block_Is_Exposed_Invariantly()
    {
        // the variance that applies to a block is its own intersected with the one holding it, so a block
        // inside an invariant one is exposed invariantly however many cultures the owner has.
        IContentType outerType = VariantBlockType();
        IContentType innerType = InvariantBlockType();

        var result = BlockElementResolver.ResolveLiveCultures(
            [
                Segment(Guid.NewGuid(), outerType, "da-dk"),
                Segment(Guid.NewGuid(), innerType, [null]),
            ],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(outerType, innerType));

        CollectionAssert.AreEqual(new[] { "da-dk" }, result);
    }

    [Test]
    public void A_Block_Exposed_Only_In_A_Segment_Is_Not_Live()
    {
        // the block's type does not vary by segment, so it is only exposed for the default segment - an entry
        // naming one does not make it visible, and rendering ignores it the same way.
        IContentType blockType = VariantBlockType();
        Guid blockKey = Guid.NewGuid();

        var result = BlockElementResolver.ResolveLiveCultures(
            [SegmentWithExpose(blockKey, blockType, [new BlockItemVariation(blockKey, "da-dk", "mobile")])],
            ContentVariation.Culture,
            propertyVariesByCulture: false,
            propertyCulture: null,
            propertySegment: null,
            ContentTypes(blockType));

        Assert.IsEmpty(result);
    }

    [Test]
    public void A_Block_In_A_Segmented_Property_Slot_Is_Live_In_Its_Culture()
    {
        // the segment applying to a block is its own type's intersected with the one holding it, so an
        // invariant-by-segment block stays exposed for the default segment whatever slot it sits in.
        IContentType blockType = VariantBlockType();
        Guid blockKey = Guid.NewGuid();

        var result = BlockElementResolver.ResolveLiveCultures(
            [Segment(blockKey, blockType, "da-dk")],
            ContentVariation.CultureAndSegment,
            propertyVariesByCulture: true,
            propertyCulture: "da-dk",
            propertySegment: "mobile",
            ContentTypes(blockType));

        CollectionAssert.AreEqual(new[] { "da-dk" }, result);
    }

    [Test]
    public void The_Block_Is_Found_In_Whichever_Stored_Slot_Holds_It()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyWith(
            Slot("da-dk", null, "da-draft", "da-published"),
            Slot("en-us", null, "en-draft", "en-published"));

        BlockElementResolver.StoredBlock? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey, "en-draft", "en-published"), EmptyPropertyEditors(), blockKey, ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.AreEqual("en-us", result.Culture);
        Assert.IsNull(result.Segment);
    }

    [Test]
    public void The_Slots_Segment_Is_Carried_Through()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyWith(Slot("da-dk", "mobile", "draft", "published"));

        BlockElementResolver.StoredBlock? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey, "draft", "published"), EmptyPropertyEditors(), blockKey, ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.AreEqual("mobile", result.Segment);
    }

    [Test]
    public void A_Block_Missing_From_The_Published_Version_Has_No_Published_Path()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyWith(Slot(null, null, "draft", "published"));

        BlockElementResolver.StoredBlock? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey, "draft"), EmptyPropertyEditors(), blockKey, ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DraftPath);
        Assert.IsNull(result.PublishedPath);
    }

    [Test]
    public void A_Block_Removed_From_The_Draft_Still_Transfers_From_The_Published_Version()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyWith(Slot(null, null, "draft", "published"));

        BlockElementResolver.StoredBlock? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey, "published"), EmptyPropertyEditors(), blockKey, ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.PublishedPath);
        Assert.AreSame(result.PublishedPath, result.DraftPath);
    }

    [Test]
    public void A_Non_Publishable_Owners_Single_Stored_Value_Serves_As_Both_Versions()
    {
        // media and members have no published/draft split, so the one stored value is the live one - the
        // published slot must not be consulted, or a block would look unpublished when it is simply current.
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyWith(Slot(null, null, "current", publishedValue: null));

        BlockElementResolver.StoredBlock? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey, "current"), EmptyPropertyEditors(), blockKey, ownerIsPublishable: false);

        Assert.IsNotNull(result);
        Assert.AreSame(result.DraftPath, result.PublishedPath);
    }

    [Test]
    public void No_Slot_Holding_The_Block_Yields_Nothing()
    {
        Guid blockKey = Guid.NewGuid();
        IProperty property = PropertyWith(Slot(null, null, "draft", "published"));

        BlockElementResolver.StoredBlock? result = BlockElementResolver.FindStoredBlock(
            property, EditorFinding(blockKey), EmptyPropertyEditors(), blockKey, ownerIsPublishable: true);

        Assert.IsNull(result);
    }

    [Test]
    public void The_Block_Is_Found_In_Whichever_Property_Holds_It()
    {
        Guid blockKey = Guid.NewGuid();
        IDataValueEditor blockEditor = BlockValueEditorFinding(blockKey, "second-draft");

        var result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [
                PropertyOf("blocks", variesByCulture: false, Slot(null, null, "first-draft", "first-published")),
                PropertyOf("blocks", variesByCulture: false, Slot(null, null, "second-draft", "second-published")),
            ],
            PropertyEditors(("blocks", blockEditor)),
            blockKey,
            ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.AreEqual("second-draft", result.Value.Property.Values.First().EditedValue);
    }

    [Test]
    public void Properties_That_Are_Not_Block_Editors_Are_Passed_Over()
    {
        Guid blockKey = Guid.NewGuid();
        IDataValueEditor blockEditor = BlockValueEditorFinding(blockKey, "draft");

        var result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [
                PropertyOf("textbox", variesByCulture: false, Slot(null, null, "draft", "published")),
                PropertyOf("blocks", variesByCulture: false, Slot(null, null, "draft", "published")),
            ],
            PropertyEditors(("blocks", blockEditor), ("textbox", Mock.Of<IDataValueEditor>())),
            blockKey,
            ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.AreEqual("blocks", result.Value.Property.PropertyType.PropertyEditorAlias);
    }

    [Test]
    public void The_Property_The_Block_Was_Found_In_Comes_Back_With_It()
    {
        // the property's own variance decides which cultures the block can be live in, so the match has to
        // carry it: taking any other property's variance would silently resolve the wrong cultures.
        Guid blockKey = Guid.NewGuid();
        IDataValueEditor blockEditor = BlockValueEditorFinding(blockKey, "variant-draft");

        var result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [
                PropertyOf("blocks", variesByCulture: false, Slot(null, null, "invariant-draft", "invariant-published")),
                PropertyOf("blocks", variesByCulture: true, Slot("da-dk", null, "variant-draft", "variant-published")),
            ],
            PropertyEditors(("blocks", blockEditor)),
            blockKey,
            ownerIsPublishable: true);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Value.Property.PropertyType.VariesByCulture());
        Assert.AreEqual("da-dk", result.Value.Block.Culture);
    }

    [Test]
    public void No_Property_Holding_The_Block_Yields_Nothing()
    {
        Guid blockKey = Guid.NewGuid();

        var result = BlockElementResolver.FindStoredBlockInAnyProperty(
            [PropertyOf("blocks", variesByCulture: false, Slot(null, null, "draft", "published"))],
            PropertyEditors(("blocks", BlockValueEditorFinding(blockKey))),
            blockKey,
            ownerIsPublishable: true);

        Assert.IsNull(result);
    }

    [Test]
    public void A_Mandatory_Culture_Present_In_The_Valid_Set_Is_Not_Missing()
    {
        ILanguage[] languages = [Language("da-dk", mandatory: true), Language("en-us", mandatory: false)];

        Assert.IsFalse(BlockElementResolver.MandatoryCultureMissing(languages, ["da-dk", "en-us"]));
    }

    [Test]
    public void A_Mandatory_Culture_Absent_From_The_Valid_Set_Is_Missing()
    {
        ILanguage[] languages = [Language("da-dk", mandatory: true), Language("en-us", mandatory: false)];

        Assert.IsTrue(BlockElementResolver.MandatoryCultureMissing(languages, ["en-us"]));
    }

    [Test]
    public void Nothing_Is_Missing_When_No_Language_Is_Mandatory()
    {
        ILanguage[] languages = [Language("da-dk", mandatory: false), Language("en-us", mandatory: false)];

        Assert.IsFalse(BlockElementResolver.MandatoryCultureMissing(languages, []));
    }

    [Test]
    public void Mandatory_Cultures_Are_Matched_Regardless_Of_Casing()
    {
        ILanguage[] languages = [Language("da-DK", mandatory: true)];

        Assert.IsFalse(BlockElementResolver.MandatoryCultureMissing(languages, ["DA-dk"]));
    }

    private static IContentType VariantBlockType()
        => new ContentTypeBuilder().WithKey(Guid.NewGuid()).WithIsElement(true).WithContentVariation(ContentVariation.Culture).Build();

    private static IContentType InvariantBlockType()
        => new ContentTypeBuilder().WithKey(Guid.NewGuid()).WithIsElement(true).WithContentVariation(ContentVariation.Nothing).Build();

    private static IReadOnlyDictionary<Guid, IContentType> ContentTypes(params IContentType[] contentTypes)
        => contentTypes.DistinctBy(contentType => contentType.Key).ToDictionary(contentType => contentType.Key);

    private static PropertyEditorCollection EmptyPropertyEditors()
        => new(new DataEditorCollection(Enumerable.Empty<IDataEditor>));

    private static IProperty PropertyWith(params IPropertyValue[] values)
        => Mock.Of<IProperty>(property => property.Values == values);

    private static IProperty PropertyOf(string editorAlias, bool variesByCulture, params IPropertyValue[] values)
    {
        IPropertyType propertyType = Mock.Of<IPropertyType>(type =>
            type.PropertyEditorAlias == editorAlias &&
            type.Variations == (variesByCulture ? ContentVariation.Culture : ContentVariation.Nothing));

        return Mock.Of<IProperty>(property => property.Values == values && property.PropertyType == propertyType);
    }

    private static PropertyEditorCollection PropertyEditors(params (string Alias, IDataValueEditor ValueEditor)[] editors)
    {
        IDataEditor[] dataEditors = editors
            .Select(editor => Mock.Of<IDataEditor>(x => x.Alias == editor.Alias && x.GetValueEditor() == editor.ValueEditor))
            .ToArray();

        return new PropertyEditorCollection(new DataEditorCollection(() => dataEditors));
    }

    /// <summary>
    ///     A value editor that is a block value editor, holding the block only in the values named.
    /// </summary>
    private static IDataValueEditor BlockValueEditorFinding(Guid blockKey, params object[] valuesHoldingTheBlock)
    {
        var editor = new Mock<IDataValueEditor>();
        editor
            .As<IBlockValueEditor>()
            .Setup(x => x.GetBlockValue(It.IsAny<object?>()))
            .Returns((object? value) => value is not null && valuesHoldingTheBlock.Contains(value)
                ? BlockValueHolding(blockKey)
                : null);

        return editor.Object;
    }

    private static IPropertyValue Slot(string? culture, string? segment, object? editedValue, object? publishedValue)
        => Mock.Of<IPropertyValue>(value =>
            value.Culture == culture &&
            value.Segment == segment &&
            value.EditedValue == editedValue &&
            value.PublishedValue == publishedValue);

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

    private static BlockValue BlockValueHolding(Guid blockKey)
    {
        var blockValue = new BlockListValue();
        blockValue.ContentData.Add(new BlockItemData(blockKey, Guid.NewGuid(), "block"));
        return blockValue;
    }

    private static BlockElementResolver.BlockLevel Segment(Guid blockKey, IContentType blockType, params string?[] exposedCultures)
        => SegmentWithExpose(blockKey, blockType, exposedCultures.Select(culture => new BlockItemVariation(blockKey, culture, null)).ToArray());

    private static BlockElementResolver.BlockLevel SegmentWithExpose(Guid blockKey, IContentType blockType, BlockItemVariation[] expose)
        => new(new BlockItemData(blockKey, blockType.Key, blockType.Alias), expose);

    private static ILanguage Language(string isoCode, bool mandatory)
        => Mock.Of<ILanguage>(language => language.IsoCode == isoCode && language.IsMandatory == mandatory);

}
