using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class BlockEditorVarianceHandlerTests
{
    private record BlockPropertyValueConfig(string Alias, string? Culture, string? Segment, object? Value);

    private record AlignedPropertyValueConfig(ContentVariation Variation, string? Culture, string Alias, object? Value, string? Segment = null);

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
    public async Task AlignedExposeVarianceAsync_Retains_Owning_Property_Culture_When_Expected_Invariant()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose =
            [
                new() { ContentKey = element.Key, Culture = "da-DK", Segment = "danish" },
                new() { ContentKey = element.Key, Culture = "en-US", Segment = "english" },
            ],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue, "en-US");

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.IsNull(variation.Culture);
            Assert.AreEqual("english", variation.Segment);
        });
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Falls_Back_To_Default_Culture_When_No_Entry_For_Owning_Property_Culture()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var element = PublishedElement(ContentVariation.Culture);
        var blockValue = new BlockListValue
        {
            Expose =
            [
                new() { ContentKey = element.Key, Culture = "da-DK", Segment = "danish" },
            ],
        };

        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue, "en-US");

        var variation = result.Single();
        Assert.Multiple(() =>
        {
            Assert.IsNull(variation.Culture);
            Assert.AreEqual("danish", variation.Segment);
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
    public async Task AlignedPropertyVarianceAsync_Retains_Owning_Property_Culture_When_Culture_Variance_Is_Disabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            new BlockPropertyValue { Culture = "en-US", Value = "English" },
            culture: "en-US");

        Assert.IsNotNull(result);
        Assert.Multiple(() =>
        {
            Assert.IsNull(result.Culture);
            Assert.AreEqual("English", result.Value);
        });
    }

    [Test]
    public async Task AlignedPropertyVarianceAsync_Falls_Back_To_Default_Culture_When_No_Value_For_Owning_Property_Culture()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            new BlockPropertyValue { Culture = "da-DK", Value = "Danish" },
            culture: "en-US");

        Assert.IsNotNull(result);
        Assert.Multiple(() =>
        {
            Assert.IsNull(result.Culture);
            Assert.AreEqual("Danish", result.Value);
        });
    }

    [Test]
    public async Task AlignedPropertyVarianceAsync_Prefers_Owning_Property_Culture_Over_Default_Culture()
    {
        IList<BlockPropertyValue> result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Nothing,
            ContentVariation.Nothing,
            [
                new() { Alias = "text", Culture = "da-DK", Value = "Danish" },
                new() { Alias = "text", Culture = "en-US", Value = "English" },
            ],
            culture: "en-US");

        Assert.AreEqual(1, result.Count);
        Assert.Multiple(() =>
        {
            Assert.IsNull(result.First().Culture);
            Assert.AreEqual("English", result.First().Value);
        });
    }

    [Test]
    public async Task AlignedPropertyVarianceAsync_Assigns_Owning_Property_Culture_When_Culture_Variance_Is_Enabled()
    {
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Culture,
            ContentVariation.Culture,
            new BlockPropertyValue { Culture = null, Value = "Shared" },
            culture: "en-US");

        Assert.IsNotNull(result);
        Assert.AreEqual("en-US", result.Culture);
    }

    [Test]
    public void AlignExposeVariance_Retains_Expose_For_Block_Without_Values()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var contentDataKey = Guid.NewGuid();
        var expose = CreateBlockItemVariations((contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, [], expose);

        ExecuteAlignExposeVariance(owner, blockValue);

        Assert.AreEqual(1, blockValue.Expose.Count);
        Assert.IsNull(blockValue.Expose.First().Culture);
    }

    [Test]
    public void AlignExposeVariance_Retains_Segments_For_Block_Without_Values()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var contentDataKey = Guid.NewGuid();
        var expose = CreateBlockItemVariations(
            (contentDataKey, "da-DK", "segment-one"),
            (contentDataKey, "da-DK", "segment-two"));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, [], expose);

        ExecuteAlignExposeVariance(owner, blockValue);

        Assert.AreEqual(2, blockValue.Expose.Count);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(blockValue.Expose.All(e => e.Culture is null));
            Assert.IsTrue(blockValue.Expose.Any(e => e.Segment == "segment-one"));
            Assert.IsTrue(blockValue.Expose.Any(e => e.Segment == "segment-two"));
        });
    }

    [Test]
    public void AlignExposeVariance_Aligns_Expose_For_Block_Without_Values_To_Variant_Element_Type()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, [], expose);

        ExecuteAlignExposeVariance(owner, blockValue, "en-US");

        Assert.AreEqual(1, blockValue.Expose.Count);
        Assert.AreEqual("en-US", blockValue.Expose.First().Culture);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Retains_Value_For_Aligned_Culture()
    {
        var propertyValues = CreatePropertyValues(
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "text", "Danish"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "en-US", "text", "English"));

        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, "en-US");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result.First().Culture);
            Assert.AreEqual("English", result.First().Value);
        });
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Falls_Back_To_Default_Culture_When_No_Value_For_Aligned_Culture()
    {
        var propertyValues = CreatePropertyValues(
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "text", "Danish"));

        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, "en-US");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result.First().Culture);
            Assert.AreEqual("Danish", result.First().Value);
        });
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Retains_Invariant_Value_Over_Culture_Values()
    {
        var propertyValues = CreatePropertyValues(
            new AlignedPropertyValueConfig(ContentVariation.Nothing, null, "text", "Invariant"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "text", "Danish"));

        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, null);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result.First().Culture);
            Assert.AreEqual("Invariant", result.First().Value);
        });
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Aligns_Each_Property_Independently()
    {
        var propertyValues = CreatePropertyValues(
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "one", "One in Danish"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "en-US", "one", "One in English"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "two", "Two in Danish"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "en-US", "two", "Two in English"));

        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, "en-US");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("One in English", result.First(v => v.Alias == "one").Value);
            Assert.AreEqual("Two in English", result.First(v => v.Alias == "two").Value);
        });
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Aligns_Each_Segment_Independently()
    {
        var propertyValues = CreatePropertyValues(
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "text", "Danish", "one"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "en-US", "text", "English", "one"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "da-DK", "text", "Danish", "two"),
            new AlignedPropertyValueConfig(ContentVariation.Nothing, "en-US", "text", "English", "two"));

        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, "en-US");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("English", result.First(v => v.Segment == "one").Value);
            Assert.AreEqual("English", result.First(v => v.Segment == "two").Value);
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
        BlockPropertyValue propertyValue,
        string? culture = null)
    {
        IList<BlockPropertyValue> result = await ExecuteAlignedPropertyVarianceAsync(
            ownerVariation,
            propertyTypeVariation,
            [propertyValue],
            culture);
        return result.SingleOrDefault();
    }

    private static async Task<IList<BlockPropertyValue>> ExecuteAlignedPropertyVarianceAsync(
        ContentVariation ownerVariation,
        ContentVariation propertyTypeVariation,
        IList<BlockPropertyValue> propertyValues,
        string? culture = null)
    {
        var owner = PublishedElement(ownerVariation);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        return await subject.AlignedPropertyVarianceAsync(
            propertyValues,
            PublishedContentType(propertyTypeVariation),
            owner,
            culture);
    }

    private static IPublishedContentType PublishedContentType(ContentVariation propertyTypeVariation)
    {
        var contentTypeMock = new Mock<IPublishedContentType>();
        contentTypeMock
            .Setup(m => m.GetPropertyType(It.IsAny<string>()))
            .Returns(PublishedPropertyType(propertyTypeVariation));
        return contentTypeMock.Object;
    }

    private static async Task<IEnumerable<BlockItemVariation>> ExecuteAlignedExposeVarianceAsync(
        IPublishedElement owner,
        IPublishedElement element,
        BlockListValue blockValue,
        string? culture = null)
    {
        var subject = BlockEditorVarianceHandler("da-DK", element);
        return await subject.AlignedExposeVarianceAsync(blockValue, owner, element, culture);
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
        return new BlockEditorVarianceHandler(languageServiceMock.Object, contentTypeServiceMock.Object, Mock.Of<IVariationContextAccessor>());
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

    private static void ExecuteAlignExposeVariance(IPublishedElement owner, BlockListValue blockValue, string? culture = null)
    {
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue, culture);
    }

    private static List<BlockPropertyValue> CreatePropertyValues(params (ContentVariation variation, string? culture)[] configs) =>
        configs.Select(c => new BlockPropertyValue
        {
            Culture = c.culture,
            PropertyType = CreatePropertyType(c.variation),
        }).ToList();

    private static List<BlockPropertyValue> CreatePropertyValues(params AlignedPropertyValueConfig[] configs) =>
        configs.Select(c => new BlockPropertyValue
        {
            Alias = c.Alias,
            Culture = c.Culture,
            Segment = c.Segment,
            Value = c.Value,
            PropertyType = CreatePropertyType(c.Variation),
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

    private static ILanguage CreateLanguage(string isoCode, string? fallbackIsoCode = null)
    {
        var builder = new LanguageBuilder()
            .WithCultureInfo(isoCode);

        if (fallbackIsoCode is not null)
        {
            builder.WithFallbackLanguageIsoCode(fallbackIsoCode);
        }

        return builder.Build();
    }
}
