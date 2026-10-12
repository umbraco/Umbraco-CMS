// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Cache;

[TestFixture]
public class DatabaseServerMessengerNotificationHandlerTests
{
    private readonly List<string> _calls = new();
    private Mock<IServerMessenger> _messenger = null!;
    private Mock<IRepositoryCacheVersionService> _cacheVersionService = null!;
    private DatabaseServerMessengerNotificationHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _calls.Clear();
        _messenger = new Mock<IServerMessenger>();
        _messenger.Setup(x => x.SendMessages()).Callback(() => _calls.Add(nameof(IServerMessenger.SendMessages)));
        _cacheVersionService = new Mock<IRepositoryCacheVersionService>();
        _cacheVersionService
            .Setup(x => x.FlushCacheUpdatesAsync())
            .Callback(() => _calls.Add(nameof(IRepositoryCacheVersionService.FlushCacheUpdatesAsync)))
            .Returns(Task.CompletedTask);

        _sut = new DatabaseServerMessengerNotificationHandler(
            _messenger.Object,
            Mock.Of<IUmbracoDatabaseFactory>(),
            NullLogger<DatabaseServerMessengerNotificationHandler>.Instance,
            Mock.Of<IRuntimeState>(),
            _cacheVersionService.Object);
    }

    [Test]
    public async Task HandleAsync_RequestEnd_WritesInstructionsBeforePublishingCacheVersions()
    {
        await _sut.HandleAsync(new UmbracoRequestEndNotification(Mock.Of<IUmbracoContext>()), CancellationToken.None);

        Assert.That(
            _calls,
            Is.EqualTo(new[] { nameof(IServerMessenger.SendMessages), nameof(IRepositoryCacheVersionService.FlushCacheUpdatesAsync) }));
    }

    [Test]
    public async Task HandleAsync_RequestEnd_PublishesCacheVersions_WhenNoInstructionsWereBatched()
    {
        _messenger.Setup(x => x.SendMessages());

        await _sut.HandleAsync(new UmbracoRequestEndNotification(Mock.Of<IUmbracoContext>()), CancellationToken.None);

        _cacheVersionService.Verify(x => x.FlushCacheUpdatesAsync(), Times.Once);
    }
}
