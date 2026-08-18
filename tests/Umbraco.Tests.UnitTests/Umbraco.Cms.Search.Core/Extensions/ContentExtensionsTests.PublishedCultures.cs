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
    public void PublishedCultures_ReturnsPublishedCultures_ForCultureVariantContent(params string[] cultures)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(ContentVariation.Culture);

        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(m => m.PublishedCultures).Returns(cultures);
        contentMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);

        var result = contentMock.Object.PublishedCultures();
        Assert.That(result, Is.EquivalentTo(cultures));
    }

    [Test]
    public void PublishedCultures_ReturnsInvariantCulture_ForCultureInvariantContent()
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(ContentVariation.Nothing);

        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);

        var result = contentMock.Object.PublishedCultures();
        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.Null);
        });
    }

    [Test]
    public void PublishedCultures_ReturnsInvariantCulture_ForMedia()
    {
        var result = Mock.Of<IMedia>().PublishedCultures();
        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.Null);
        });
    }

    [Test]
    public void PublishedCultures_ReturnsInvariantCulture_ForMembers()
    {
        var result = Mock.Of<IMember>().PublishedCultures();
        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.Null);
        });
    }
}
