using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class UmbracoBuilderExtensionsTests
{
    [Test]
    [Customization]
    public void AddNotificationsFromAssembly_Should_AddNotificationHandler_To_ServicesCollection(IUmbracoBuilder sut)
    {
        sut.AddNotificationsFromAssembly<CustomizationAttribute>();

        var expectedHandlerType = typeof(INotificationHandler<ContentPublishedNotification>);
        var handler = sut.Services.SingleOrDefault(x => x.ServiceType == expectedHandlerType);
        Assert.NotNull(handler);
        Assert.That(handler.ImplementationType, Is.EqualTo(typeof(StubNotificationHandler)));
    }

    [Test]
    [Customization]
    public void AddNotificationsFromAssembly_Should_AddAsyncNotificationHandler_To_ServicesCollection(
        IUmbracoBuilder sut)
    {
        sut.AddNotificationsFromAssembly<CustomizationAttribute>();

        var expectedHandlerType = typeof(INotificationAsyncHandler<ContentPublishedNotification>);
        var handler = sut.Services.SingleOrDefault(x => x.ServiceType == expectedHandlerType);
        Assert.NotNull(handler);
        Assert.That(handler.ImplementationType, Is.EqualTo(typeof(StubNotificationHandler)));
    }

    private class CustomizationAttribute : AutoDataAttribute
    {
        public CustomizationAttribute()
            : base(() =>
        {
            var fixture = new Fixture();

            var stub = new UmbracoBuildStub();
            fixture.Inject((IUmbracoBuilder)stub);

            return fixture;
        })
        {
        }
    }

    private class UmbracoBuildStub : IUmbracoBuilder
    {
        public UmbracoBuildStub() => Services = new ServiceCollection();

        public IServiceCollection Services { get; }

        public IConfiguration Config { get; }

        public TypeLoader TypeLoader { get; }

        public ILoggerFactory BuilderLoggerFactory { get; }

        public IHostingEnvironment BuilderHostingEnvironment { get; }

        public IProfiler Profiler { get; }

        public AppCaches AppCaches { get; }

        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder : ICollectionBuilder => default;

        public void Build()
        {
        }
    }

    private class StubNotificationHandler
        : INotificationHandler<ContentPublishedNotification>,
            INotificationAsyncHandler<ContentPublishedNotification>
    {
        public Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;

        public void Handle(ContentPublishedNotification notification)
        {
        }
    }
}
