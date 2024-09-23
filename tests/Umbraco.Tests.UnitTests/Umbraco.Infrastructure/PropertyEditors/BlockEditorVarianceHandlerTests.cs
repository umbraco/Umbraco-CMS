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
        var subject = BlockEditorVarianceHandler("da-DK");
        await subject.AlignPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(ContentVariation.Culture),
            PublishedElement(ContentVariation.Culture));
        Assert.AreEqual("da-DK", propertyValue.Culture);
    }

    [Test]
    public async Task Removes_Default_Culture_When_Culture_Variance_Is_Disabled()
    {
        var propertyValue = new BlockPropertyValue { Culture = "da-DK" };
        var subject = BlockEditorVarianceHandler("da-DK");
        await subject.AlignPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(ContentVariation.Nothing),
            PublishedElement(ContentVariation.Nothing));
        Assert.AreEqual(null, propertyValue.Culture);
    }

    [Test]
    public async Task Ignores_NonDefault_Culture_When_Culture_Variance_Is_Disabled()
    {
        var propertyValue = new BlockPropertyValue { Culture = "en-US" };
        var subject = BlockEditorVarianceHandler("da-DK");
        await subject.AlignPropertyVarianceAsync(
            propertyValue,
            PublishedPropertyType(ContentVariation.Nothing),
            PublishedElement(ContentVariation.Nothing));
        Assert.AreEqual("en-US", propertyValue.Culture);
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
        var elementMock = new Mock<IPublishedElement>();
        elementMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);
        return elementMock.Object;
    }

    private static BlockEditorVarianceHandler BlockEditorVarianceHandler(string defaultLanguageIsoCode)
    {
        var languageServiceMock = new Mock<ILanguageService>();
        languageServiceMock.Setup(m => m.GetDefaultIsoCodeAsync()).ReturnsAsync(defaultLanguageIsoCode);
        return new BlockEditorVarianceHandler(languageServiceMock.Object);
    }
}
