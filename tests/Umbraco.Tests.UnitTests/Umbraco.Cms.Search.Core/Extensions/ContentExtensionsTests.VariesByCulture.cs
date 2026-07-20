using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Search.Core.Extensions;

public partial class ContentExtensionsTests
{
    [TestCase(ContentVariation.Culture, true)]
    [TestCase(ContentVariation.Segment, false)]
    [TestCase(ContentVariation.Nothing, false)]
    [TestCase(ContentVariation.CultureAndSegment, true)]
    public void VariesByCulture_ReturnsCultureVariance_ForCultureVariantContent(ContentVariation contentVariation, bool expectedResult)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(contentVariation);

        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);

        var result = contentMock.Object.VariesByCulture();
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void VariesByCulture_ReturnsFalse_ForMedia()
    {
        var contentMock = new Mock<IMedia>();

        var result = contentMock.Object.VariesByCulture();
        Assert.That(result, Is.False);
    }

    [Test]
    public void VariesByCulture_ReturnsFalse_ForMembers()
    {
        var contentMock = new Mock<IMember>();

        var result = contentMock.Object.VariesByCulture();
        Assert.That(result, Is.False);
    }
}
