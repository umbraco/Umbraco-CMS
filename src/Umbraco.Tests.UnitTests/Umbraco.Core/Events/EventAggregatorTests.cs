using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Builder;
using Umbraco.Core.Events;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Common.Builder;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Events
{
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
        public async Task CanPublishEvents()
        {
            _builder.AddNotificationHandler<Notification, NotificationHandlerA>();
            _builder.AddNotificationHandler<Notification, NotificationHandlerB>();
            _builder.AddNotificationHandler<Notification, NotificationHandlerC>();
            var provider = _builder.Services.BuildServiceProvider();

            var notification = new Notification();
            var aggregator = provider.GetService<IEventAggregator>();
            await aggregator.PublishAsync(notification);

            Assert.AreEqual(A + B + C, notification.SubscriberCount);
        }

        public class Notification : INotification
        {
            public int SubscriberCount { get; set; }
        }

        public class NotificationHandlerA : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
            {
                notification.SubscriberCount += A;
                return Task.CompletedTask;
            }
        }

        public class NotificationHandlerB : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
            {
                notification.SubscriberCount += B;
                return Task.CompletedTask;
            }
        }

        public class NotificationHandlerC : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
            {
                notification.SubscriberCount += C;
                return Task.CompletedTask;
            }
        }
    }
}
