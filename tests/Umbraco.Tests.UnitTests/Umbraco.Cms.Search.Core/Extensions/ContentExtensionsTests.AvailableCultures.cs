using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Search.Core.Extensions;

public partial class ContentExtensionsTests
{
    [TestCase("de-DE")]
    [TestCase("en-US", "da-DK")]
    [TestCase]
    public void AvailableCultures_ReturnsAvailableCultures_ForCultureVariantContent(params string[] cultures)
        => AvailableCultures_ReturnsAvailableCultures_ForCultureVariant<IContent>(cultures);

    [Test]
    public void AvailableCultures_ReturnsInvariantCulture_ForCultureInvariantContent()
        => AvailableCultures_ReturnsInvariantCulture_ForCultureInvariant<IContent>();

    [Test]
    public void AvailableCultures_ReturnsInvariantCulture_ForMedia()
        => AvailableCultures_ReturnsInvariantCulture_ForCultureInvariant<IMedia>();

    [Test]
    public void AvailableCultures_ReturnsInvariantCulture_ForMember()
        => AvailableCultures_ReturnsInvariantCulture_ForCultureInvariant<IMember>();

    private void AvailableCultures_ReturnsAvailableCultures_ForCultureVariant<TContent>(params string[] cultures)
        where TContent : class, IContentBase
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(ContentVariation.Culture);

        var contentMock = new Mock<TContent>();
        contentMock.SetupGet(m => m.AvailableCultures).Returns(cultures);
        contentMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);

        var result = contentMock.Object.AvailableCultures();
        Assert.That(result, Is.EquivalentTo(cultures));
    }

    private void AvailableCultures_ReturnsInvariantCulture_ForCultureInvariant<TContent>()
        where TContent : class, IContentBase
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(ContentVariation.Nothing);

        var contentMock = new Mock<TContent>();
        contentMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);

        var result = contentMock.Object.AvailableCultures();
        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.Null);
        });
    }
}
