using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Scoping;

[TestFixture]
public class ScopedNotificationPublisherTests
{
    [Test]
    public void ScopeUsesInjectedNotificationPublisher()
    {
        var notificationPublisherMock = new Mock<IScopedNotificationPublisher>();
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);

        using (ICoreScope scope = scopeProvider.CreateScope(notificationPublisher: notificationPublisherMock.Object))
        {
            scope.Notifications.Publish(Mock.Of<INotification>());
            scope.Notifications.PublishCancelable(Mock.Of<ICancelableNotification>());

            notificationPublisherMock.Verify(x => x.Publish(It.IsAny<INotification>()), Times.Once);
            notificationPublisherMock.Verify(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()), Times.Once);

            // Ensure that the custom scope provider is till used in inner scope.
            using (ICoreScope innerScope = scopeProvider.CreateScope())
            {
                innerScope.Notifications.Publish(Mock.Of<INotification>());
                innerScope.Notifications.PublishCancelable(Mock.Of<ICancelableNotification>());

                notificationPublisherMock.Verify(x => x.Publish(It.IsAny<INotification>()), Times.Exactly(2));
                notificationPublisherMock.Verify(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()), Times.Exactly(2));
            }

            // Ensure scope exit is not called until outermost scope is being disposed
            notificationPublisherMock.Verify(x => x.ScopeExit(It.IsAny<bool>()), Times.Never());
        }

        notificationPublisherMock.Verify(x => x.ScopeExit(It.IsAny<bool>()), Times.Once());

        // Ensure that the event aggregator isn't used directly.
        eventAggregatorMock.Verify(x => x.Publish(It.IsAny<INotification>()), Times.Never);
        eventAggregatorMock.Verify(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()), Times.Never);
    }

    [Test]
    public void SpecifyingNotificationPublishInInnerScopeCausesError()
    {
        var notificationPublisherMock = new Mock<IScopedNotificationPublisher>();
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);

        using (var scope = scopeProvider.CreateScope())
        {
            Assert.Throws<ArgumentException>(() =>
                scopeProvider.CreateScope(notificationPublisher: notificationPublisherMock.Object));
        }
    }

    [Test]
    public async Task Disposing_Scope_Asynchronously_Publishes_Scoped_Notifications_Asynchronously()
    {
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);
        ICoreScope? ambientScopeDuringPublish = null;
        eventAggregatorMock
            .Setup(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Yield();
                ambientScopeDuringPublish = scopeProvider.AmbientScope;
            });

        await using (ICoreScope scope = scopeProvider.CreateScope())
        {
            scope.Notifications.Publish(Mock.Of<INotification>());
            scope.Complete();
        }

        eventAggregatorMock.Verify(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()), Times.Once);
        eventAggregatorMock.Verify(x => x.Publish<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>()), Times.Never);
        Assert.IsNull(ambientScopeDuringPublish);
        Assert.IsNull(scopeProvider.AmbientScope);
        Assert.IsNull(scopeProvider.AmbientContext);
    }

    [Test]
    public void Disposing_Scope_Synchronously_Publishes_Scoped_Notifications_Synchronously()
    {
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);

        using (ICoreScope scope = scopeProvider.CreateScope())
        {
            scope.Notifications.Publish(Mock.Of<INotification>());
            scope.Complete();
        }

        eventAggregatorMock.Verify(x => x.Publish<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>()), Times.Once);
        eventAggregatorMock.Verify(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Disposing_Uncompleted_Scope_Asynchronously_Does_Not_Publish_Scoped_Notifications()
    {
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);

        await using (ICoreScope scope = scopeProvider.CreateScope())
        {
            scope.Notifications.Publish(Mock.Of<INotification>());
        }

        eventAggregatorMock.Verify(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()), Times.Never);
        eventAggregatorMock.Verify(x => x.Publish<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>()), Times.Never);
        Assert.IsNull(scopeProvider.AmbientScope);
    }

    [Test]
    public async Task Disposing_Nested_Scope_Asynchronously_Publishes_Scoped_Notifications_Only_When_The_Outermost_Scope_Exits()
    {
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);

        await using (ICoreScope scope = scopeProvider.CreateScope())
        {
            await using (ICoreScope innerScope = scopeProvider.CreateScope())
            {
                innerScope.Notifications.Publish(Mock.Of<INotification>());
                innerScope.Complete();
            }

            eventAggregatorMock.Verify(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.AreSame(scope, scopeProvider.AmbientScope);
            scope.Complete();
        }

        eventAggregatorMock.Verify(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsNull(scopeProvider.AmbientScope);
    }

    [Test]
    public async Task Disposing_Scope_Asynchronously_Throws_Handler_Exceptions_After_Exiting_The_Scope()
    {
        var scopeProvider = GetScopeProvider(out var eventAggregatorMock);
        var handlerException = new InvalidOperationException("Handler failed.");
        eventAggregatorMock
            .Setup(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(handlerException);

        AggregateException? exception = Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await using ICoreScope scope = scopeProvider.CreateScope();
            scope.Notifications.Publish(Mock.Of<INotification>());
            scope.Complete();
        });

        Assert.AreSame(handlerException, exception!.InnerExceptions.Single());
        Assert.IsNull(scopeProvider.AmbientScope);
    }

    [Test]
    public async Task Exiting_Scope_Asynchronously_Uses_Overridden_Synchronous_Publishing()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        var publisher = new SynchronousScopedNotificationPublisher(eventAggregatorMock.Object);
        publisher.Publish(Mock.Of<INotification>());

        await publisher.ScopeExitAsync(true);

        Assert.AreEqual(1, publisher.PublishedCount);
        eventAggregatorMock.Verify(x => x.PublishAsync<INotification, INotificationHandler>(It.IsAny<IEnumerable<INotification>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class SynchronousScopedNotificationPublisher : ScopedNotificationPublisher
    {
        public SynchronousScopedNotificationPublisher(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        public int PublishedCount { get; private set; }

        protected override void PublishScopedNotifications(IList<INotification> notifications)
            => PublishedCount += notifications.Count;
    }

    private ScopeProvider GetScopeProvider(out Mock<IEventAggregator> eventAggregatorMock)
    {
        var loggerFactory = NullLoggerFactory.Instance;

        var fileSystems = new FileSystems(
            loggerFactory,
            Mock.Of<IIOHelper>(),
            Options.Create(new GlobalSettings()),
            Mock.Of<IHostingEnvironment>());

        var mediaFileManager = new MediaFileManager(
            Mock.Of<IFileSystem>(),
            Mock.Of<IMediaPathScheme>(),
            loggerFactory.CreateLogger<MediaFileManager>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<Lazy<ICoreScopeProvider>>());

        eventAggregatorMock = new Mock<IEventAggregator>();

        return new ScopeProvider(
            new AmbientScopeStack(),
            new AmbientScopeContextStack(),
            Mock.Of<IDistributedLockingMechanismFactory>(),
            Mock.Of<IUmbracoDatabaseFactory>(),
            fileSystems,
            new TestOptionsMonitor<CoreDebugSettings>(new CoreDebugSettings()),
            mediaFileManager,
            loggerFactory,

            eventAggregatorMock.Object);
    }
}
