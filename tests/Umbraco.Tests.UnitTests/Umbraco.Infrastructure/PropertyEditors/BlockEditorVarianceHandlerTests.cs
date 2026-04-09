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
    private record BlockPropertyValueConfig(string Alias, string? Culture, string? Segment, object? Value);

    [Test]
    public async Task AlignedPropertyVarianceAsync_Assigns_Default_Culture_When_Culture_Variance_Is_Enabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Culture,
            ContentVariation.Culture,
            new BlockPropertyValue { Culture = null });
        Assert.IsNotNull(result);
        Assert.AreEqual("da-DK", result.Culture);
    }

    [Test]
    public async Task AlignedPropertyVarianceAsync_Removes_Default_Culture_When_Culture_Variance_Is_Disabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            new BlockPropertyValue { Culture = "da-DK" });
        Assert.IsNotNull(result);
        Assert.IsNull(result.Culture);
    }

    [Test]
    public async Task AlignedPropertyVarianceAsync_Ignores_NonDefault_Culture_When_Culture_Variance_Is_Disabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            new BlockPropertyValue { Culture = "en-US" });
        Assert.IsNull(result);
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Returns_Empty_When_No_Matching_Variations()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose = [new() { ContentKey = Guid.NewGuid(), Culture = "da-DK" }],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Assigns_Default_Culture_When_Variations_Are_Invariant()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose = [new() { ContentKey = element.Key, Culture = null, Segment = null }],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(element.Key, variation.ContentKey);
            Assert.AreEqual("da-DK", variation.Culture);
        });
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Removes_Default_Culture_When_Expected_Invariant()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose = [new() { ContentKey = element.Key, Culture = "da-DK" }],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(element.Key, variation.ContentKey);
            Assert.IsNull(variation.Culture);
        });
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Filters_NonDefault_Culture_When_Expected_Invariant()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose =
            [
                new() { ContentKey = element.Key, Culture = "da-DK" },
                new() { ContentKey = element.Key, Culture = "en-US" },
            ],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(element.Key, variation.ContentKey);
            Assert.IsNull(variation.Culture);
        });
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Returns_Unchanged_When_Already_Variant()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose = [new() { ContentKey = element.Key, Culture = "da-DK" }],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(element.Key, variation.ContentKey);
            Assert.AreEqual("da-DK", variation.Culture);
        });
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Returns_Unchanged_When_Already_Invariant()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose = [new() { ContentKey = element.Key, Culture = null }],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(element.Key, variation.ContentKey);
            Assert.IsNull(variation.Culture);
        });
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Preserves_Segment_When_Assigning_Culture()
    {
        var owner = PublishedElement(ContentVariation.CultureAndSegment);
        var element = PublishedElement(ContentVariation.CultureAndSegment);
        var blockValue = new BlockListValue
        {
            Expose = [new() { ContentKey = element.Key, Culture = null, Segment = "my-segment" }],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.AreEqual("da-DK", variation.Culture);
            Assert.AreEqual("my-segment", variation.Segment);
        });
    }

    [Test]
    public void AlignExposeVariance_Can_Align_Invariance()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(new BlockPropertyValueConfig("one", null, null, "Value one"));
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
        var values = CreateBlockPropertyValues(new BlockPropertyValueConfig("one", "en-US", "segment-one", "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);

        ExecuteAlignExposeVariance(owner, blockValue);

        Assert.Multiple(() =>
        {
            var alignedExpose = blockValue.Expose.First();
            Assert.AreEqual("en-US", alignedExpose.Culture);
            Assert.AreEqual("segment-one", alignedExpose.Segment);
        });
    }

    [Test]
    public void AlignExposeVariance_Can_Handle_Variant_Element_Type_With_All_Invariant_Block_Values()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(new BlockPropertyValueConfig("one", null, null, "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);

        ExecuteAlignExposeVariance(owner, blockValue);

        Assert.AreEqual("da-DK", blockValue.Expose.First().Culture);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Removes_NonDefault_Culture_Values()
    {
        var propertyValues = CreatePropertyValues(
            (ContentVariation.Nothing, "da-DK"),
            (ContentVariation.Nothing, "en-US"));
        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Nothing, propertyValues, null);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result.First().Culture);
        });
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
            new BlockPropertyValueConfig("one", "da-DK", null, "Value one"),
            new BlockPropertyValueConfig("two", "da-DK", null, "Value two"));
        var expose = CreateBlockItemVariations(
            (contentDataKey, "da-DK", null),
            (contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.AreEqual(1, blockValue.Expose.Count);
    }

    [Test]
    public void AlignExposeVariance_Skips_When_ElementType_Not_Found()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var unknownContentTypeKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(new BlockPropertyValueConfig("one", "da-DK", null, "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = CreateBlockListValue(contentDataKey, unknownContentTypeKey, values, expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockValue.Expose.Count);
            Assert.IsNull(blockValue.Expose.First().Culture);
        });
    }

    [Test]
    public void AlignExposeVariance_Handles_Multiple_ContentData_Items()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey1 = Guid.NewGuid();
        var contentDataKey2 = Guid.NewGuid();
        var values1 = CreateBlockPropertyValues(new BlockPropertyValueConfig("one", "da-DK", null, "Value one"));
        var values2 = CreateBlockPropertyValues(new BlockPropertyValueConfig("two", "en-US", null, "Value two"));
        var expose = CreateBlockItemVariations(
            (contentDataKey1, null, null),
            (contentDataKey2, null, null));
        var blockValue = CreateBlockListValue(
            owner.ContentType.Key,
            [
                (contentDataKey1, values1),
                (contentDataKey2, values2)
            ],
            expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, blockValue.Expose.Count);
            Assert.IsTrue(blockValue.Expose.Any(e => e.Culture == "da-DK"));
            Assert.IsTrue(blockValue.Expose.Any(e => e.Culture == "en-US"));
        });
    }

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

    private static async Task<IEnumerable<BlockItemVariation>> ExecuteAlignedExposeVarianceAsync(
        IPublishedElement owner,
        IPublishedElement element,
        BlockListValue blockValue)
    {
        var subject = BlockEditorVarianceHandler("da-DK", element);
        return await subject.AlignedExposeVarianceAsync(blockValue, owner, element);
    }

    private static IPublishedElement PublishedElement(ContentVariation variation)
    {
        var contentTypeMock = new Mock<IPublishedContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(variation);
        contentTypeMock.SetupGet(m => m.Key).Returns(Guid.NewGuid());
        var elementMock = new Mock<IPublishedElement>();
        elementMock.SetupGet(m => m.Key).Returns(Guid.NewGuid());
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

            // Return null for unknown content types - this simulates real behavior where
            // IContentTypeService.Get() can return null for non-existent content types.
            // The production code handles this with .WhereNotNull() (see BlockEditorVarianceHandler.cs:172).
            // This is tested by AlignExposeVariance_Skips_When_ElementType_Not_Found.
            return null!;
        });
        return new BlockEditorVarianceHandler(languageServiceMock.Object, contentTypeServiceMock.Object);
    }

    private static IPublishedPropertyType PublishedPropertyType(ContentVariation variation)
    {
        var propertyTypeMock = new Mock<IPublishedPropertyType>();
        propertyTypeMock.SetupGet(m => m.Variations).Returns(variation);
        return propertyTypeMock.Object;
    }

    private static List<BlockPropertyValue> CreateBlockPropertyValues(params BlockPropertyValueConfig[] configs) =>
        configs.Select(c => new BlockPropertyValue
        {
            Alias = c.Alias,
            Culture = c.Culture,
            Segment = c.Segment,
            Value = c.Value,
        }).ToList();

    private static List<BlockItemVariation> CreateBlockItemVariations(params (Guid contentKey, string? culture, string? segment)[] configs) =>
        configs.Select(c => new BlockItemVariation
        {
            ContentKey = c.contentKey,
            Culture = c.culture,
            Segment = c.segment,
        }).ToList();

    private static BlockListValue CreateBlockListValue(Guid contentDataKey, Guid contentTypeKey, List<BlockPropertyValue> values, List<BlockItemVariation> expose) =>
        new()
        {
            ContentData = [new() { Key = contentDataKey, ContentTypeKey = contentTypeKey, Values = values }],
            Expose = expose,
        };

    private static void ExecuteAlignExposeVariance(IPublishedElement owner, BlockListValue blockValue)
    {
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
    }

    private static List<BlockPropertyValue> CreatePropertyValues(params (ContentVariation variation, string? culture)[] configs) =>
        configs.Select(c => new BlockPropertyValue
        {
            Culture = c.culture,
            PropertyType = CreatePropertyType(c.variation),
        }).ToList();

    private static IPropertyType CreatePropertyType(ContentVariation variation)
    {
        var propertyTypeMock = new Mock<IPropertyType>();
        propertyTypeMock.SetupGet(m => m.Variations).Returns(variation);
        return propertyTypeMock.Object;
    }

    private static async Task<IList<BlockPropertyValue>> ExecuteAlignPropertyVarianceAsync(
        ContentVariation ownerVariation,
        List<BlockPropertyValue> propertyValues,
        string? culture)
    {
        var owner = PublishedElement(ownerVariation);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        return await subject.AlignPropertyVarianceAsync(propertyValues, culture);
    }

    private static BlockListValue CreateBlockListValue(Guid contentTypeKey, List<(Guid key, List<BlockPropertyValue> values)> contentData, List<BlockItemVariation> expose) =>
        new()
        {
            ContentData = contentData.Select(cd => new BlockItemData { Key = cd.key, ContentTypeKey = contentTypeKey, Values = cd.values }).ToList(),
            Expose = expose,
        };
}
