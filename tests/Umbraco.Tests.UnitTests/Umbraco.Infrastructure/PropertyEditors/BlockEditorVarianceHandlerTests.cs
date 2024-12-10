using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

// TODO KJA: more tests for BlockEditorVarianceHandler
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
        var blockValue = new BlockListValue
        {
            ContentData =
            [
                new()
                {
                    Key = contentDataKey,
                    ContentTypeKey = owner.ContentType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "one", Culture = null, Segment = null, Value = "Value one" }
                    ]
                }
            ],
            Expose = [new() { ContentKey = contentDataKey, Culture = "da-DK" }]
        };

        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);

        Assert.AreEqual(null, blockValue.Expose.First().Culture);
    }

    [Test]
    public void AlignExpose_Can_Align_Variance()
    {
        var owner = PublishedElement(ContentVariation.CultureAndSegment);
        var contentDataKey = Guid.NewGuid();
        var blockValue = new BlockListValue
        {
            ContentData =
            [
                new()
                {
                    Key = contentDataKey,
                    ContentTypeKey = owner.ContentType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "one", Culture = "en-US", Segment = "segment-one", Value = "Value one" }
                    ]
                }
            ],
            Expose = [new() { ContentKey = contentDataKey, Culture = null, Segment = null }]
        };

        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);

        Assert.Multiple(() =>
        {
            var alignedExpose = blockValue.Expose.First();
            Assert.AreEqual("en-US", alignedExpose.Culture);
            Assert.AreEqual("segment-one", alignedExpose.Segment);
        });
    }

    [Test]
    public void AlignExpose_Can_Handle_Variant_Element_Type_With_All_Invariant_Block_Values()
    {
        var owner = PublishedElement(ContentVariation.Culture);
        var contentDataKey = Guid.NewGuid();
        var blockValue = new BlockListValue
        {
            ContentData =
            [
                new()
                {
                    Key = contentDataKey,
                    ContentTypeKey = owner.ContentType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "one", Culture = null, Segment = null, Value = "Value one" }
                    ]
                }
            ],
            Expose = [new() { ContentKey = contentDataKey, Culture = "da-DK" }]
        };

        var subject = BlockEditorVarianceHandler("da-DK", owner);
        subject.AlignExposeVariance(blockValue);

        Assert.AreEqual("da-DK", blockValue.Expose.First().Culture);
    }

    private static IPublishedPropertyType PublishedPropertyType(ContentVariation variation)
    {
        var propertyTypeMock = new Mock<IPublishedPropertyType>();
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
        contentTypeServiceMock.Setup(c => c.Get(element.ContentType.Key)).Returns(elementType.Object);
        return new BlockEditorVarianceHandler(languageServiceMock.Object, contentTypeServiceMock.Object);
    }
}
