// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Events;

[TestFixture]
public class EventAggregatorTests
{
    private const int A = 3;
    private const int B = 5;
    private const int C = 7;
    private IUmbracoBuilder _builder;

    [SetUp]
    public void Setup()
    {
        var register = TestHelper.GetServiceCollection();
        _builder = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());
    }

    [Test]
    public void CanPublish()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerA>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerB>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerC>();
        _builder.AddNotificationHandler<ChildNotification, NotificationHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notification = new Notification();
        aggregator.Publish(notification);

        var childNotification = new ChildNotification();
        aggregator.Publish(childNotification);

        Assert.AreEqual(A + B + C, notification.SubscriberCount, "Notification should be handled by all 3 registered INotificationHandlers (A, B and C).");
        Assert.AreEqual(A, childNotification.SubscriberCount, "ChildNotification should only be handled by a single registered INotificationHandler (A).");
    }

    [Test]
    public async Task CanPublishAsync()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerA>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerB>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerC>();
        _builder.AddNotificationAsyncHandler<ChildNotification, NotificationAsyncHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notification = new Notification();
        await aggregator.PublishAsync(notification);

        var childNotification = new ChildNotification();
        await aggregator.PublishAsync(childNotification);

        Assert.AreEqual(A + B + C, notification.SubscriberCount, "Notification should be handled by all 3 registered INotificationAsyncHandlers (A, B and C).");
        Assert.AreEqual(A, childNotification.SubscriberCount, "ChildNotification should only be handled by a single registered INotificationAsyncHandler (A).");
    }

    [Test]
    public void CanPublishMultiple()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerA>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerB>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerC>();
        _builder.AddNotificationHandler<ChildNotification, NotificationHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notifications = new Notification[]
        {
            new Notification(),
            new Notification(),
            new ChildNotification()
        };
        aggregator.Publish(notifications);

        Assert.AreEqual(A + B + C, notifications[0].SubscriberCount, "Notification should be handled by all 3 registered INotificationHandlers (A, B and C).");
        Assert.AreEqual(A + B + C, notifications[1].SubscriberCount, "Notification should be handled by all 3 registered INotificationHandlers (A, B and C).");
        Assert.AreEqual(A, notifications[2].SubscriberCount, "ChildNotification should only be handled by a single registered INotificationHandler (A).");
    }

    [Test]
    public async Task CanPublishMultipleAsync()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerA>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerB>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerC>();
        _builder.AddNotificationAsyncHandler<ChildNotification, NotificationAsyncHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notifications = new Notification[]
        {
            new Notification(),
            new Notification(),
            new ChildNotification()
        };
        await aggregator.PublishAsync(notifications);

        Assert.AreEqual(A + B + C, notifications[0].SubscriberCount, "Notification should be handled by all 3 registered INotificationAsyncHandlers (A, B and C).");
        Assert.AreEqual(A + B + C, notifications[1].SubscriberCount, "Notification should be handled by all 3 registered INotificationAsyncHandlers (A, B and C).");
        Assert.AreEqual(A, notifications[2].SubscriberCount, "ChildNotification should only be handled by a single registered INotificationAsyncHandler (A).");
    }

    [Test]
    public void CanPublishDistributedCache()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerA>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerB>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerC>();
        _builder.AddNotificationHandler<ChildNotification, NotificationHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notification = new Notification();
        aggregator.Publish<INotification, IDistributedCacheNotificationHandler>(notification);

        var childNotification = new ChildNotification();
        aggregator.Publish<INotification, IDistributedCacheNotificationHandler>(childNotification);

        Assert.AreEqual(B, notification.SubscriberCount, "Notification should only be handled by a single registered IDistributedCacheNotificationHandler (B).");
        Assert.AreEqual(0, childNotification.SubscriberCount, "ChildNotification should not be handled, since it has no registered IDistributedCacheNotificationHandler.");
    }

    [Test]
    public async Task CanPublishDistributedCacheAsync()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerA>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerB>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerC>();
        _builder.AddNotificationAsyncHandler<ChildNotification, NotificationAsyncHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notification = new Notification();
        await aggregator.PublishAsync<INotification, IDistributedCacheNotificationHandler>(notification);

        var childNotification = new ChildNotification();
        await aggregator.PublishAsync<INotification, IDistributedCacheNotificationHandler>(childNotification);

        Assert.AreEqual(B, notification.SubscriberCount, "Notification should only be handled by a single registered IDistributedCacheNotificationHandler (B).");
        Assert.AreEqual(0, childNotification.SubscriberCount, "ChildNotification should not be handled, since it has no registered IDistributedCacheNotificationHandler.");
    }

    [Test]
    public void CanPublishMultipleDistributedCache()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerA>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerB>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerC>();
        _builder.AddNotificationHandler<ChildNotification, NotificationHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notifications = new Notification[]
        {
            new Notification(),
            new Notification(),
            new ChildNotification()
        };
        aggregator.Publish<INotification, IDistributedCacheNotificationHandler>(notifications);

        Assert.AreEqual(B, notifications[0].SubscriberCount, "Notification should only be handled by a single registered IDistributedCacheNotificationHandler (B).");
        Assert.AreEqual(B, notifications[1].SubscriberCount, "Notification should only be handled by a single registered IDistributedCacheNotificationHandler (B).");
        Assert.AreEqual(0, notifications[2].SubscriberCount, "ChildNotification should not be handled, since it has no registered IDistributedCacheNotificationHandler.");
    }

    [Test]
    public async Task CanPublishMultipleDistributedCacheAsync()
    {
        _builder.Services.AddScoped<Adder>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerA>();
        _builder.AddNotificationAsyncHandler<Notification, NotificationAsyncHandlerB>();
        _builder.AddNotificationHandler<Notification, NotificationHandlerC>();
        _builder.AddNotificationAsyncHandler<ChildNotification, NotificationAsyncHandlerA>();

        var provider = _builder.Services.BuildServiceProvider();
        var aggregator = provider.GetService<IEventAggregator>();

        var notifications = new Notification[]
        {
            new Notification(),
            new Notification(),
            new ChildNotification()
        };
        await aggregator.PublishAsync<INotification, IDistributedCacheNotificationHandler>(notifications);

        Assert.AreEqual(B, notifications[0].SubscriberCount, "Notification should only be handled by a single registered IDistributedCacheNotificationHandler (B).");
        Assert.AreEqual(B, notifications[1].SubscriberCount, "Notification should only be handled by a single registered IDistributedCacheNotificationHandler (B).");
        Assert.AreEqual(0, notifications[2].SubscriberCount, "ChildNotification should not be handled, since it has no registered IDistributedCacheNotificationHandler.");
    }

    public class Notification : INotification
    {
        public int SubscriberCount { get; set; }
    }

    public class ChildNotification : Notification
    { }

    public class NotificationHandlerA : INotificationHandler<Notification>
    {
        public void Handle(Notification notification) => notification.SubscriberCount += A;
    }

    public class NotificationHandlerB : IDistributedCacheNotificationHandler<Notification>
    {
        public void Handle(Notification notification) => notification.SubscriberCount += B;
    }

    public class NotificationHandlerC : INotificationHandler<Notification>
    {
        private readonly Adder _adder;

        public NotificationHandlerC(Adder adder) => _adder = adder;

        public void Handle(Notification notification) =>
            notification.SubscriberCount = _adder.Add(notification.SubscriberCount, C);
    }

    public class NotificationAsyncHandlerA : INotificationAsyncHandler<Notification>
    {
        public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.SubscriberCount += A;
            return Task.CompletedTask;
        }
    }

    public class NotificationAsyncHandlerB : INotificationAsyncHandler<Notification>, IDistributedCacheNotificationHandler
    {
        public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.SubscriberCount += B;
            return Task.CompletedTask;
        }
    }

    public class NotificationAsyncHandlerC : INotificationAsyncHandler<Notification>
    {
        private readonly Adder _adder;

        public NotificationAsyncHandlerC(Adder adder) => _adder = adder;

        public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.SubscriberCount = _adder.Add(notification.SubscriberCount, C);
            return Task.CompletedTask;
        }
    }

    public class Adder
    {
        public int Add(int a, int b) => a + b;
    }
}
