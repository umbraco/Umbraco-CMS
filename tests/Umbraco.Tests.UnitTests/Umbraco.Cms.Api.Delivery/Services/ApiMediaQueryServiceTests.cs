// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Services;

[TestFixture]
public class ApiMediaQueryServiceTests
{
    private Mock<IPublishedMediaCache> _mediaCache = null!;
    private Mock<IMediaNavigationQueryService> _navigationQueryService = null!;
    private Mock<IPublishedMediaStatusFilteringService> _statusFilteringService = null!;

    [SetUp]
    public void SetUp()
    {
        _mediaCache = new Mock<IPublishedMediaCache>();
        _navigationQueryService = new Mock<IMediaNavigationQueryService>();
        _statusFilteringService = new Mock<IPublishedMediaStatusFilteringService>();
    }

    private ApiMediaQueryService CreateSut() => new(
        _mediaCache.Object,
        NullLogger<ApiMediaQueryService>.Instance,
        _navigationQueryService.Object,
        _statusFilteringService.Object);

    private IPublishedContent SetUpParentWithSingleChild(Guid parentKey, Guid childKey)
    {
        var child = new Mock<IPublishedContent>();
        child.Setup(x => x.Key).Returns(childKey);

        IEnumerable<Guid> childKeys = new[] { childKey };
        _navigationQueryService.Setup(x => x.TryGetChildrenKeys(parentKey, out childKeys)).Returns(true);
        _statusFilteringService
            .Setup(x => x.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>()))
            .Returns(new[] { child.Object });

        var parent = new Mock<IPublishedContent>();
        parent.Setup(x => x.Key).Returns(parentKey);
        return parent.Object;
    }

    // Only the "children:" prefix may be removed from the selector. The media key here deliberately begins
    // with 'e' — a character that also appears in "children:" — so the test fails if prefix removal ever
    // regresses into character-set trimming that eats into the key itself.
    [Test]
    public void Can_Resolve_ChildrenOf_By_Key_When_Key_Starts_With_A_Prefix_Character()
    {
        var parentKey = new Guid("eeeeeeee-1111-1111-1111-111111111111");
        var childKey = new Guid("aaaaaaaa-2222-2222-2222-222222222222");

        IPublishedContent parent = SetUpParentWithSingleChild(parentKey, childKey);
        _mediaCache.Setup(x => x.GetById(parentKey)).Returns(parent);

        var attempt = CreateSut().ExecuteQuery(
            $"children:{parentKey}",
            [],
            [],
            skip: 0,
            take: 100);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Result.Total, Is.EqualTo(1));
            Assert.That(attempt.Result.Items, Is.EqualTo(new[] { childKey }));
        });
    }

    // Same invariant for path-based resolution: the leading path segment "news" begins with characters that
    // also appear in the "children:" prefix. Removing the prefix must leave the segment intact ("news"), so
    // the node stays resolvable by path — a character-set trim would reduce it to "ws" and lose the node.
    [Test]
    public void Can_Resolve_ChildrenOf_By_Path_When_Segment_Starts_With_A_Prefix_Character()
    {
        var newsKey = new Guid("cccccccc-1111-1111-1111-111111111111");
        var childKey = new Guid("aaaaaaaa-2222-2222-2222-222222222222");

        var newsNode = new Mock<IPublishedContent>();
        newsNode.Setup(x => x.Key).Returns(newsKey);
        newsNode.Setup(x => x.Name).Returns("news");

        IEnumerable<Guid> rootKeys = new[] { newsKey };
        _navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeys)).Returns(true);
        _mediaCache.Setup(x => x.GetById(false, newsKey)).Returns(newsNode.Object);

        IEnumerable<Guid> childKeys = new[] { childKey };
        _navigationQueryService.Setup(x => x.TryGetChildrenKeys(newsKey, out childKeys)).Returns(true);

        var child = new Mock<IPublishedContent>();
        child.Setup(x => x.Key).Returns(childKey);
        _statusFilteringService
            .Setup(x => x.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>()))
            .Returns(new[] { child.Object });

        var attempt = CreateSut().ExecuteQuery(
            "children:news",
            [],
            [],
            skip: 0,
            take: 100);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Result.Total, Is.EqualTo(1));
            Assert.That(attempt.Result.Items, Is.EqualTo(new[] { childKey }));
        });
    }
}
