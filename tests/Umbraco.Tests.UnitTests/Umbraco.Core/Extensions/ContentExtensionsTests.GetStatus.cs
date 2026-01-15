using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

public partial class ContentExtensionsTests
{
    [Test]
    public void GetStatus_WhenTrashed_ReturnsTrashed()
    {
        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(c => c.Trashed).Returns(true);
        var result = contentMock.Object.GetStatus(new ContentScheduleCollection());
        Assert.AreEqual(ContentStatus.Trashed, result);
    }

    [TestCase(true, ContentStatus.Published)]
    [TestCase(false, ContentStatus.Unpublished)]
    public void GetStatus_WithEmptySchedule_ReturnsPublishState(bool published, ContentStatus expectedStatus)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);
        mock.SetupGet(c => c.Published).Returns(published);

        var result = mock.Object.GetStatus(new ContentScheduleCollection());
        Assert.AreEqual(expectedStatus, result);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(60)]
    [TestCase(120)]
    [TestCase(1000)]
    public void GetStatus_WithPendingExpiry_ForInvariant_ReturnsExpired(int minutesFromExpiry)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);

        var schedule = ContentScheduleCollection.CreateWithEntry(null, DateTime.UtcNow.AddMinutes(-1 * minutesFromExpiry));
        var result = mock.Object.GetStatus(schedule);
        Assert.AreEqual(ContentStatus.Expired, result);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(60)]
    [TestCase(120)]
    [TestCase(1000)]
    public void GetStatus_WithPendingRelease_ForInvariant_ReturnsAwaitingRelease(int minutesUntilRelease)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);

        var schedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(minutesUntilRelease), null);
        var result = mock.Object.GetStatus(schedule);
        Assert.AreEqual(ContentStatus.AwaitingRelease, result);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(60)]
    [TestCase(120)]
    [TestCase(1000)]
    public void GetStatus_WithPastReleaseAndFutureExpiry_ForInvariant_ReturnsPublishedState(int minutes)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);
        mock.SetupGet(c => c.Published).Returns(true);

        var schedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-1 * minutes), DateTime.UtcNow.AddMinutes(minutes));
        var result = mock.Object.GetStatus(schedule);
        Assert.AreEqual(ContentStatus.Published, result);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(60)]
    [TestCase(120)]
    [TestCase(1000)]
    public void GetStatus_WithPendingExpiry_ForVariant_ReturnsExpired(int minutesFromExpiry)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Culture);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);

        var schedule = ContentScheduleCollection.CreateWithEntry("en-US", null, DateTime.UtcNow.AddMinutes(-1 * minutesFromExpiry));
        var result = mock.Object.GetStatus(schedule, "en-US");
        Assert.AreEqual(ContentStatus.Expired, result);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(60)]
    [TestCase(120)]
    [TestCase(1000)]
    public void GetStatus_WithPendingRelease_ForVariant_ReturnsAwaitingRelease(int minutesUntilRelease)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Culture);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);

        var schedule = ContentScheduleCollection.CreateWithEntry("en-US", DateTime.UtcNow.AddMinutes(minutesUntilRelease), null);
        var result = mock.Object.GetStatus(schedule, "en-US");
        Assert.AreEqual(ContentStatus.AwaitingRelease, result);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(60)]
    [TestCase(120)]
    [TestCase(1000)]
    public void GetStatus_WithPastReleaseAndFutureExpiry_ForVariant_ReturnsPublishedState(int minutes)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Culture);
        var mock = new Mock<IContent>();
        mock.SetupGet(c => c.ContentType).Returns(contentTypeMock.Object);
        mock.SetupGet(c => c.Published).Returns(true);

        var schedule = ContentScheduleCollection.CreateWithEntry("en-US", DateTime.UtcNow.AddMinutes(-1 * minutes), DateTime.UtcNow.AddMinutes(minutes));
        var result = mock.Object.GetStatus(schedule, "en-US");
        Assert.AreEqual(ContentStatus.Published, result);
    }
}
