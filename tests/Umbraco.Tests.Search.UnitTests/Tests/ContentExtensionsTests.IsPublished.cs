using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;

namespace Umbraco.Tests.Search.UnitTests.Tests;

public partial class ContentExtensionsTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void IsPublished_ReturnsPublished_ForContent(bool published)
    {
        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(m => m.Published).Returns(published);

        var result = contentMock.Object.IsPublished();
        Assert.That(result, Is.EqualTo(published));
    }

    [Test]
    public void IsPublished_ReturnsFalse_ForMedia()
    {
        var contentMock = new Mock<IMedia>();

        var result = contentMock.Object.IsPublished();
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsPublished_ReturnsFalse_ForMembers()
    {
        var contentMock = new Mock<IMember>();

        var result = contentMock.Object.IsPublished();
        Assert.That(result, Is.False);
    }
}
