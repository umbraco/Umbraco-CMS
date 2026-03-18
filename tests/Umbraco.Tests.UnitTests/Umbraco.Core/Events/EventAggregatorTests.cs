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

/// <summary>
/// Contains unit tests for the <see cref="EventAggregator"/> class in the <c>Umbraco.Core.Events</c> namespace.
/// </summary>
[TestFixture]
public class EventAggregatorTests
{
    private const int A = 3;
    private const int B = 5;
    private const int C = 7;
    private IUmbracoBuilder _builder;

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var register = TestHelper.GetServiceCollection();
        _builder = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());
    }

    /// <summary>
    /// Verifies that the event aggregator correctly publishes notifications to all registered handlers.
    /// Specifically, ensures that a <see cref="Notification"/> is handled by all registered handlers (including async handlers),
    /// and that a <see cref="ChildNotification"/> is only handled by handlers registered for its type.
    /// </summary>
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

    /// <summary>
    /// Tests that the event aggregator can publish notifications asynchronously
    /// and that the correct handlers are invoked for each notification type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that multiple notifications can be published and handled by their respective handlers.
    /// Verifies that all registered handlers receive the notifications as expected.
    /// </summary>
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

    /// <summary>
    /// Tests that multiple notifications can be published asynchronously and handled by their respective handlers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that the event aggregator publishes notifications only to handlers implementing
    /// <see cref="IDistributedCacheNotificationHandler"/>. Ensures that when publishing a notification,
    /// only the appropriate handler(s) receive it, and that notifications without a registered handler
    /// are not handled. Specifically, checks that <see cref="Notification"/> is handled by the correct
    /// handler, and <see cref="ChildNotification"/> is not handled since it lacks a registered
    /// <see cref="IDistributedCacheNotificationHandler"/>.
    /// </summary>
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

    /// <summary>
    /// Verifies that the event aggregator can asynchronously publish notifications to handlers implementing the <see cref="IDistributedCacheNotificationHandler"/> interface.
    /// Specifically, ensures that only registered distributed cache notification handlers receive the notification, and that notifications without registered handlers are not handled.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that multiple notifications can be published to handlers implementing IDistributedCacheNotificationHandler.
    /// Verifies that only the appropriate handlers receive the notifications and that ChildNotification is not handled.
    /// </summary>
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

    /// <summary>
    /// Tests that multiple notifications can be published asynchronously using the distributed cache notification handlers.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Represents a test notification used within the <see cref="EventAggregatorTests"/> unit tests.
    /// This class is used to simulate notifications for testing the event aggregator behavior.
    /// </summary>
    public class Notification : INotification
    {
    /// <summary>
    /// Gets or sets the number of subscribers.
    /// </summary>
        public int SubscriberCount { get; set; }
    }

    /// <summary>
    /// Represents a test helper class for child notification events used in event aggregation unit tests.
    /// </summary>
    public class ChildNotification : Notification
    { }

    /// <summary>
    /// A test notification handler used as a mock implementation in event aggregator unit tests.
    /// </summary>
    public class NotificationHandlerA : INotificationHandler<Notification>
    {
    /// <summary>
    /// Handles the specified notification by incrementing its SubscriberCount.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
        public void Handle(Notification notification) => notification.SubscriberCount += A;
    }

    /// <summary>
    /// Represents a test notification handler (Handler B) used for verifying event aggregation behavior in unit tests.
    /// </summary>
    public class NotificationHandlerB : IDistributedCacheNotificationHandler<Notification>
    {
    /// <summary>
    /// Handles the specified notification by incrementing its SubscriberCount by B.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
        public void Handle(Notification notification) => notification.SubscriberCount += B;
    }

    /// <summary>
    /// A test notification handler (type C) used in unit tests for the event aggregator.
    /// </summary>
    public class NotificationHandlerC : INotificationHandler<Notification>
    {
        private readonly Adder _adder;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHandlerC"/> class.
    /// </summary>
    /// <param name="adder">The adder delegate used by the handler.</param>
        public NotificationHandlerC(Adder adder) => _adder = adder;

    /// <summary>
    /// Handles the specified notification by updating its SubscriberCount.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
        public void Handle(Notification notification) =>
            notification.SubscriberCount = _adder.Add(notification.SubscriberCount, C);
    }

    /// <summary>
    /// Represents a test asynchronous notification handler (A) used in unit tests for the event aggregator.
    /// </summary>
    public class NotificationAsyncHandlerA : INotificationAsyncHandler<Notification>
    {
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
        public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.SubscriberCount += A;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// An asynchronous notification handler (B) used for event aggregator unit tests.
    /// </summary>
    public class NotificationAsyncHandlerB : INotificationAsyncHandler<Notification>, IDistributedCacheNotificationHandler
    {
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
        public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.SubscriberCount += B;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Represents a mock asynchronous notification handler used for testing the event aggregator.
    /// This class is intended for use within unit tests.
    /// </summary>
    public class NotificationAsyncHandlerC : INotificationAsyncHandler<Notification>
    {
        private readonly Adder _adder;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationAsyncHandlerC"/> class.
    /// </summary>
    /// <param name="adder">An <see cref="Adder"/> instance used by the handler for its operations.</param>
        public NotificationAsyncHandlerC(Adder adder) => _adder = adder;

    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
        public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
        {
            notification.SubscriberCount = _adder.Add(notification.SubscriberCount, C);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Represents a simple class used for addition operations in event aggregation unit tests.
    /// This class is primarily intended for testing purposes within the EventAggregatorTests.
    /// </summary>
    public class Adder
    {
    /// <summary>
    /// Adds two integers and returns the result.
    /// </summary>
    /// <param name="a">The first integer to add.</param>
    /// <param name="b">The second integer to add.</param>
    /// <returns>The sum of the two integers.</returns>
        public int Add(int a, int b) => a + b;
    }
}
