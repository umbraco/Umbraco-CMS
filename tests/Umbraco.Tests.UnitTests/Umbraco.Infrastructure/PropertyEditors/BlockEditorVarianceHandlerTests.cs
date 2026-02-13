using System.Linq;
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
    public void AlignExpose_Can_Align_Invariance()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(("one", null, null, "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, "da-DK", null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.IsNull(blockValue.Expose.First().Culture);
    }

    [Test]
    public void AlignExpose_Can_Align_Variance()
    {
        var owner = PublishedElement(ContentVariation.CultureAndSegment);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(("one", "en-US", "segment-one", "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        var alignedExpose = blockValue.Expose.First();
        Assert.AreEqual("en-US", alignedExpose.Culture);
        Assert.AreEqual("segment-one", alignedExpose.Segment);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Removes_NonDefault_Culture_Values()
    {
        var propertyValues = CreatePropertyValues(
            (ContentVariation.Nothing, "da-DK"),
            (ContentVariation.Nothing, "en-US"));
        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Nothing, propertyValues, null);
        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result.First().Culture);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Throws_When_PropertyType_Is_Null()
    {
        var propertyValues = new List<BlockPropertyValue> { new() { Culture = null, PropertyType = null! } };
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, null));
        Assert.IsTrue(ex!.Message.Contains("property type"));
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Assigns_Default_Culture_When_All_Null()
    {
        var (owner, element, blockValue) = SetupAlignedExposeTest(ContentVariation.Culture, ContentVariation.Culture, 
            elementKey => [new() { ContentKey = elementKey, Culture = null }]);
        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);
        Assert.AreEqual("da-DK", result.First().Culture);
    }

    [Test]
    public void AlignExposeVariance_Removes_Expose_When_ContentData_Is_Missing()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var blockValue = CreateBlockListValue(Guid.NewGuid(), owner.ContentType.Key, [], 
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

    [Test]
    public async Task AlignPropertyVarianceAsync_Assigns_Culture_When_PropertyType_Varies_By_Culture()
    {
        var propertyValues = CreatePropertyValues((ContentVariation.Culture, null));
        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, "en-US");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("en-US", result.First().Culture);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Keeps_Values_When_Variation_Matches()
    {
        var propertyValues = CreatePropertyValues(
            (ContentVariation.Culture, "da-DK"),
            (ContentVariation.Nothing, null));
        var result = await ExecuteAlignPropertyVarianceAsync(ContentVariation.Culture, propertyValues, null);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("da-DK", result.First().Culture);
        Assert.IsNull(result.Last().Culture);
    }

    [Test]
    public async Task AlignedPropertyVarianceAsync_Returns_As_Is_When_Variation_Matches()
    {
        var propertyValue = new BlockPropertyValue { Culture = "da-DK", Alias = "test", Value = "test value" };
        var result = await ExecuteAlignedPropertyVarianceAsync(
            ContentVariation.Culture,
            ContentVariation.Culture,
            propertyValue);
        Assert.IsNotNull(result);
        Assert.AreEqual("da-DK", result.Culture);
        Assert.AreSame(propertyValue, result);
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Returns_Empty_When_No_Matching_Expose()
    {
        var (owner, element, blockValue) = SetupAlignedExposeTest(ContentVariation.Culture, ContentVariation.Culture,
            _ => [new() { ContentKey = Guid.NewGuid(), Culture = "da-DK" }]);
        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Filters_To_Default_Culture_When_Invariant()
    {
        var (owner, element, blockValue) = SetupAlignedExposeTest(ContentVariation.Nothing, ContentVariation.Nothing,
            elementKey => [
                new() { ContentKey = elementKey, Culture = "da-DK", Segment = null },
                new() { ContentKey = elementKey, Culture = "en-US", Segment = null }
            ]);
        var result = await ExecuteAlignedExposeVarianceAsync(owner, element, blockValue);
        Assert.AreEqual(1, result.Count());
        Assert.IsNull(result.First().Culture);
    }

    [Test]
    public void AlignExposeVariance_Handles_Segment_Variations()
    {
        var owner = PublishedElement(ContentVariation.CultureAndSegment);
        var contentDataKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(
            ("one", "da-DK", "segment1", "Value one"),
            ("two", "da-DK", "segment2", "Value two"));
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.AreEqual(2, blockValue.Expose.Count);
        Assert.IsTrue(blockValue.Expose.Any(e => e.Segment == "segment1"));
        Assert.IsTrue(blockValue.Expose.Any(e => e.Segment == "segment2"));
    }

    [Test]
    public void AlignExposeVariance_Skips_When_ElementType_Not_Found()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var unknownContentTypeKey = Guid.NewGuid();
        var values = CreateBlockPropertyValues(("one", "da-DK", null, "Value one"));
        var expose = CreateBlockItemVariations((contentDataKey, null, null));
        var blockValue = new BlockListValue
        {
            ContentData = [new() { Key = contentDataKey, ContentTypeKey = unknownContentTypeKey, Values = values }],
            Expose = expose
        };
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.AreEqual(1, blockValue.Expose.Count);
        Assert.IsNull(blockValue.Expose.First().Culture);
    }

    [Test]
    public void AlignExposeVariance_Handles_Multiple_ContentData_Items()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey1 = Guid.NewGuid();
        var contentDataKey2 = Guid.NewGuid();
        var values1 = CreateBlockPropertyValues(("one", "da-DK", null, "Value one"));
        var values2 = CreateBlockPropertyValues(("two", "en-US", null, "Value two"));
        var expose = CreateBlockItemVariations(
            (contentDataKey1, null, null),
            (contentDataKey2, null, null));
        var blockValue = new BlockListValue
        {
            ContentData =
            [
                new() { Key = contentDataKey1, ContentTypeKey = owner.ContentType.Key, Values = values1 },
                new() { Key = contentDataKey2, ContentTypeKey = owner.ContentType.Key, Values = values2 }
            ],
            Expose = expose
        };
        ExecuteAlignExposeVariance(owner, blockValue);
        Assert.AreEqual(2, blockValue.Expose.Count);
        Assert.IsTrue(blockValue.Expose.Any(e => e.Culture == "da-DK"));
        Assert.IsTrue(blockValue.Expose.Any(e => e.Culture == "en-US"));
    }

    private static List<BlockPropertyValue> CreatePropertyValues(params (ContentVariation variation, string? culture)[] configs)
    {
        return configs.Select(c => new BlockPropertyValue 
        { 
            Culture = c.culture, 
            PropertyType = CreatePropertyType(c.variation) 
        }).ToList();
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

    private static (IPublishedElement owner, IPublishedElement element, BlockListValue blockValue) SetupAlignedExposeTest(
        ContentVariation ownerVariation,
        ContentVariation elementVariation,
        Func<Guid, List<BlockItemVariation>> createExpose)
    {
        var owner = PublishedElement(ownerVariation);
        var element = PublishedElement(elementVariation);
        var elementKey = element.Key;
        var blockValue = CreateBlockListValue(elementKey, owner.ContentType.Key, [], createExpose(elementKey));
        return (owner, element, blockValue);
    }

    private static async Task<IEnumerable<BlockItemVariation>> ExecuteAlignedExposeVarianceAsync(
        IPublishedElement owner,
        IPublishedElement element,
        BlockListValue blockValue)
    {
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        return await subject.AlignedExposeVarianceAsync(blockValue, owner, element);
    }

    private static List<BlockPropertyValue> CreateBlockPropertyValues(params (string alias, string? culture, string? segment, object? value)[] configs)
    {
        return configs.Select(c => new BlockPropertyValue 
        { 
            Alias = c.alias,
            Culture = c.culture, 
            Segment = c.segment,
            Value = c.value
        }).ToList();
    }

    private static List<BlockItemVariation> CreateBlockItemVariations(params (Guid contentKey, string? culture, string? segment)[] configs)
    {
        return configs.Select(c => new BlockItemVariation 
        { 
            ContentKey = c.contentKey,
            Culture = c.culture, 
            Segment = c.segment
        }).ToList();
    }

    private static void ExecuteAlignExposeVariance(IPublishedElement owner, BlockListValue blockValue)
    {
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
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

    private static BlockListValue CreateBlockListValue(Guid contentDataKey, Guid contentTypeKey, List<BlockPropertyValue> values, List<BlockItemVariation> expose)
    {
        return new BlockListValue
        {
            ContentData = [new() { Key = contentDataKey, ContentTypeKey = contentTypeKey, Values = values }],
            Expose = expose
        };
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
            var mockType = new Mock<IContentType>();
            mockType.SetupGet(e => e.Key).Returns(key);
            mockType.SetupGet(e => e.Variations).Returns(element.ContentType.Variations);
            return mockType.Object;
        });
        return new BlockEditorVarianceHandler(languageServiceMock.Object, contentTypeServiceMock.Object);
    }
}
