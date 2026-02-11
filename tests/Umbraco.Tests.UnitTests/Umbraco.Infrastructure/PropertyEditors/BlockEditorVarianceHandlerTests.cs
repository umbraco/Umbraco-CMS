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
        var propertyValue = new BlockPropertyValue { Culture = null };
        var owner = PublishedElement(ContentVariation.Culture);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        var result = await subject.AlignedPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(ContentVariation.Culture),
            owner);
        Assert.IsNotNull(result);
        Assert.AreEqual("da-DK", result.Culture);
    }

    [Test]
    public async Task Removes_Default_Culture_When_Culture_Variance_Is_Disabled()
    {
        var propertyValue = new BlockPropertyValue { Culture = "da-DK" };
        var owner = PublishedElement(ContentVariation.Nothing);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        var result = await subject.AlignedPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(ContentVariation.Nothing),
            owner);
        Assert.IsNotNull(result);
        Assert.AreEqual(null, result.Culture);
    }

    [Test]
    public async Task Ignores_NonDefault_Culture_When_Culture_Variance_Is_Disabled()
    {
        var propertyValue = new BlockPropertyValue { Culture = "en-US" };
        var owner = PublishedElement(ContentVariation.Nothing);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        var result = await subject.AlignedPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(ContentVariation.Nothing),
            owner);
        Assert.IsNull(result);
    }

    [Test]
    public void AlignExpose_Can_Align_Invariance()
    {
        var owner = PublishedElement(ContentVariation.Nothing);
        var contentDataKey = Guid.NewGuid();
        var values = new List<BlockPropertyValue> { new() { Alias = "one", Culture = null, Segment = null, Value = "Value one" } };
        var expose = new List<BlockItemVariation> { new() { ContentKey = contentDataKey, Culture = "da-DK" } };
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
        Assert.IsNull(blockValue.Expose.First().Culture);
    }

    [Test]
    public void AlignExpose_Can_Align_Variance()
    {
        var owner = PublishedElement(ContentVariation.CultureAndSegment);
        var contentDataKey = Guid.NewGuid();
        var values = new List<BlockPropertyValue> { new() { Alias = "one", Culture = "en-US", Segment = "segment-one", Value = "Value one" } };
        var expose = new List<BlockItemVariation> { new() { ContentKey = contentDataKey, Culture = null, Segment = null } };
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
        var alignedExpose = blockValue.Expose.First();
        Assert.AreEqual("en-US", alignedExpose.Culture);
        Assert.AreEqual("segment-one", alignedExpose.Segment);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Removes_NonDefault_Culture_Values()
    {
        var propertyValues = new List<BlockPropertyValue>
        {
            new() { Culture = "da-DK", PropertyType = CreatePropertyType(ContentVariation.Nothing) },
            new() { Culture = "en-US", PropertyType = CreatePropertyType(ContentVariation.Nothing) }
        };
        var subject = BlockEditorVarianceHandler("da-DK", PublishedElement(ContentVariation.Nothing));
        var result = await subject.AlignPropertyVarianceAsync(propertyValues, null);
        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result.First().Culture);
    }

    [Test]
    public async Task AlignPropertyVarianceAsync_Throws_When_PropertyType_Is_Null()
    {
        var propertyValues = new List<BlockPropertyValue>
        {
            new() { Culture = null, PropertyType = null! }
        };
        var subject = BlockEditorVarianceHandler("da-DK", PublishedElement(ContentVariation.Culture));
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await subject.AlignPropertyVarianceAsync(propertyValues, null));
        Assert.IsTrue(ex!.Message.Contains("property type"));
    }

    [Test]
    public async Task AlignedExposeVarianceAsync_Assigns_Default_Culture_When_All_Null()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var element = PublishedElement(ContentVariation.Culture);
        var elementKey = element.Key;
        var blockValue = CreateBlockListValue(elementKey, owner.ContentType.Key, [], [new() { ContentKey = elementKey, Culture = null }]);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        var result = await subject.AlignedExposeVarianceAsync(blockValue, owner, element);
        Assert.AreEqual("da-DK", result.First().Culture);
    }

    [Test]
    public void AlignExposeVariance_Removes_Expose_When_ContentData_Is_Missing()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var blockValue = CreateBlockListValue(Guid.NewGuid(), owner.ContentType.Key, [], [new() { ContentKey = Guid.NewGuid(), Culture = "da-DK" }]);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
        Assert.IsEmpty(blockValue.Expose);
    }

    [Test]
    public void AlignExposeVariance_Deduplicates_Expose_Entries()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var values = new List<BlockPropertyValue>
        {
            new() { Alias = "one", Culture = "da-DK", Segment = null, Value = "Value one" },
            new() { Alias = "two", Culture = "da-DK", Segment = null, Value = "Value two" }
        };
        var expose = new List<BlockItemVariation>
        {
            new() { ContentKey = contentDataKey, Culture = "da-DK", Segment = null },
            new() { ContentKey = contentDataKey, Culture = "da-DK", Segment = null }
        };
        var blockValue = CreateBlockListValue(contentDataKey, owner.ContentType.Key, values, expose);
        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);
        Assert.AreEqual(1, blockValue.Expose.Count);
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
