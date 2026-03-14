using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

    /// <summary>
    /// Contains unit tests for methods in the <see cref="ContentExtensions"/> class.
    /// </summary>
public partial class ContentExtensionsTests
{
    /// <summary>
    /// Tests that GetStatus returns ContentStatus.Trashed when the content is trashed.
    /// </summary>
    [Test]
    public void GetStatus_WhenTrashed_ReturnsTrashed()
    {
        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(c => c.Trashed).Returns(true);
        var result = contentMock.Object.GetStatus(new ContentScheduleCollection());
        Assert.AreEqual(ContentStatus.Trashed, result);
    }

    /// <summary>
    /// Tests that GetStatus returns the correct ContentStatus based on the published state when the schedule is empty.
    /// </summary>
    /// <param name="published">Indicates whether the content is published.</param>
    /// <param name="expectedStatus">The expected ContentStatus result.</param>
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

    /// <summary>
    /// Verifies that the <c>GetStatus</c> method returns <see cref="ContentStatus.Expired"/> when the content has a pending expiry and is invariant (not culture variant).
    /// </summary>
    /// <param name="minutesFromExpiry">The number of minutes before the expiry time to test.</param>
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

    /// <summary>
    /// Tests that GetStatus returns AwaitingRelease when there is a pending release for invariant content.
    /// </summary>
    /// <param name="minutesUntilRelease">The number of minutes until the scheduled release.</param>
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

    /// <summary>
    /// Tests that the content status is published when the release date is in the past and the expiry date is in the future for invariant content.
    /// </summary>
    /// <param name="minutes">The number of minutes to offset the release and expiry dates from the current time.</param>
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

    /// <summary>
    /// Tests that the GetStatus method returns Expired when the content variant has a pending expiry within the specified minutes.
    /// </summary>
    /// <param name="minutesFromExpiry">The number of minutes from expiry to test the status against.</param>
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

    /// <summary>
    /// Tests that GetStatus returns AwaitingRelease when there is a pending release for a variant.
    /// </summary>
    /// <param name="minutesUntilRelease">The number of minutes until the scheduled release.</param>
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

    /// <summary>
    /// Tests that the content status is published when the release date is in the past and the expiry date is in the future for a specific variant.
    /// </summary>
    /// <param name="minutes">The number of minutes to offset the release and expiry dates from the current time.</param>
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
