using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;
[TestFixture]
public class BlockEditorVarianceHandlerTests
{
    [Test]
    public async Task Assigns_Default_Culture_When_Culture_Variance_Is_Enabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Culture,
            ContentVariation.Culture,
            new BlockPropertyValue { Culture = null });
        Assert.IsNotNull(result);
        Assert.AreEqual("da-DK", result.Culture);
    }

    [Test]
    public async Task Removes_Default_Culture_When_Culture_Variance_Is_Disabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            new BlockPropertyValue { Culture = "da-DK" });
        Assert.IsNotNull(result);
        Assert.AreEqual(null, result.Culture);
    }

    [Test]
    public async Task Ignores_NonDefault_Culture_When_Culture_Variance_Is_Disabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            new BlockPropertyValue { Culture = "en-US" });
        Assert.IsNull(result);
    }

    [Test]
    public void AlignExposeVariance_Can_Align_Invariance()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(("one", null, null, "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);

        ExecuteAlignExposeVariance(owner, blockValue);

        Assert.AreEqual(null, blockValue.Expose.First().Culture);
    }

    [Test]
    public void AlignExposeVariance_Can_Align_Variance()
    {
        var owner = PublishedElement(ContentVariation.CultureAndSegment);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(("one", "en-US", "segment-one", "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);

        ExecuteAlignExposeVariance(owner, blockValue);

        Assert.Multiple(() =>
        {
            var alignedExpose = blockValue.Expose.First();
            Assert.AreEqual("en-US", alignedExpose.Culture);
            Assert.AreEqual("segment-one", alignedExpose.Segment);
    }

    [Test]
    public void AlignExposeVariance_Can_Handle_Variant_Element_Type_With_All_Invariant_Block_Values()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(("one", null, null, "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);

        ExecuteAlignExposeVariance(owner, blockValue);

    private static async Task<BlockPropertyValue?> ExecuteAlignedPropertyVarianceAsync(
        ContentVariation ownerVariation,
        ContentVariation propertyTypeVariation,
        BlockPropertyValue propertyValue)
    {
        var owner = PublishedElement(ownerVariation);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        return await subject.AlignedPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(propertyTypeVariation),
            owner);
    }

    private static BlockListValue CreateBlockListValue(Guid contentDataKey, Guid contentTypeKey, List<BlockPropertyValue> values, List<BlockItemVariation> expose)
    {
        return new BlockListValue
        {
            ContentData = [new() { Key = contentDataKey, ContentTypeKey = contentTypeKey, Values = values }],
            Expose = expose
        };
    }

    [Test]
    public void AlignExposeVariance_Removes_Exposed_Keys_When_Not_In_ContentData()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var blockValue = CreateBlockListValue(
            Guid.NewGuid(),
            owner.ContentType.Key,
            [],
            [new() { ContentKey = Guid.NewGuid(), Culture = "da-DK" }]);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.IsEmpty(blockValue.Expose);
    }

    [Test]
    public void AlignExposeVariance_Deduplicates_Expose_Entries()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(
            ("one", "da-DK", null, "Value one"),
            ("two", "da-DK", null, "Value two"));
        var expose = CreateBlockItemVariations(
            (contentDataKey, "da-DK", null),
            (contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.AreEqual(1, blockValue.Expose.Count);
    }

    private static IPublishedPropertyType PublishedPropertyType(ContentVariation variation)
    {
        var propertyTypeMock = new Mock<IPublishedPropertyType>();
        propertyTypeMock.SetupGet(m => m.Variations).Returns(variation);
        return propertyTypeMock.Object;
    }

    private static IPropertyType CreatePropertyType(ContentVariation variation)
    {
        var propertyTypeMock = new Mock<IPropertyType>();
        propertyTypeMock.SetupGet(m => m.Variations).Returns(variation);
        return propertyTypeMock.Object;
    }

    private static IPublishedElement PublishedElement(ContentVariation variation)
    {
        var contentTypeMock = new Mock<IPublishedContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(variation);
        contentTypeMock.SetupGet(m => m.Key).Returns(Guid.NewGuid());
        var elementMock = new Mock<IPublishedElement>();
        elementMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);
        return elementMock.Object;
    }

    private static BlockEditorVarianceHandler BlockEditorVarianceHandler(string defaultLanguageIsoCode, IPublishedElement element)
    {
        var languageServiceMock = new Mock<ILanguageService>();
        languageServiceMock.Setup(m => m.GetDefaultIsoCodeAsync()).ReturnsAsync(defaultLanguageIsoCode);
        var contentTypeServiceMock = new Mock<IContentTypeService>();
        var elementType = new Mock<IContentType>();
        elementType.SetupGet(e => e.Key).Returns(element.ContentType.Key);
        elementType.SetupGet(e => e.Variations).Returns(element.ContentType.Variations);
        contentTypeServiceMock.Setup(c => c.Get(It.IsAny<Guid>())).Returns((Guid key) =>
        {
            if (key == element.ContentType.Key)
            {
                return elementType.Object;
            }
            return null!; // Return null for unknown content types
        });
        return new BlockEditorVarianceHandler(languageServiceMock.Object, contentTypeServiceMock.Object);
    }

    private static List<BlockPropertyValue> CreateBlockPropertyValues(params (string alias, string? culture, string? segment, object? value)[] configs) =>
        configs.Select(c => new BlockPropertyValue
        {
            Alias = c.alias,
            Culture = c.culture,
            Segment = c.segment,
            Value = c.value,
        }).ToList();

    private static List<BlockItemVariation> CreateBlockItemVariations(params (Guid contentKey, string? culture, string? segment)[] configs) =>
        configs.Select(c => new BlockItemVariation
        {
            ContentKey = c.contentKey,
            Culture = c.culture,
            Segment = c.segment,
        }).ToList();

    private static void ExecuteAlignExposeVariance(IPublishedElement owner, BlockListValue blockValue)
    {
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
    }

    private static BlockListValue CreateBlockListValue(Guid contentDataKey, Guid contentTypeKey, List<BlockPropertyValue> values, List<BlockItemVariation> expose) =>
        new()
        {
            ContentData = [new() { Key = contentDataKey, ContentTypeKey = contentTypeKey, Values = values }],
            Expose = expose,
        };
}
