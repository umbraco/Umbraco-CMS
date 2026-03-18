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

/// <summary>
/// Contains unit tests for the <see cref="UmbracoBuilderExtensions"/> class, verifying its extension methods and behaviors.
/// </summary>
[TestFixture]
public class UmbracoBuilderExtensionsTests
{
    /// <summary>
    /// Verifies that calling <c>AddNotificationsFromAssembly</c> on the provided <see cref="IUmbracoBuilder"/> instance
    /// successfully registers a notification handler in the services collection.
    /// </summary>
    /// <param name="sut">The <see cref="IUmbracoBuilder"/> instance to which the notification handler is added.</param>
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

    /// <summary>
    /// Verifies that calling <c>AddNotificationsFromAssembly</c> on the provided <see cref="IUmbracoBuilder"/> instance
    /// correctly registers an asynchronous notification handler (<see cref="INotificationAsyncHandler{TNotification}"/>)
    /// for <see cref="ContentPublishedNotification"/> in the services collection.
    /// </summary>
    /// <param name="sut">The <see cref="IUmbracoBuilder"/> instance to which notifications are added.</param>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomizationAttribute"/> class.
    /// </summary>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoBuildStub"/> class.
    /// </summary>
        public UmbracoBuildStub() => Services = new ServiceCollection();

    /// <summary>
    /// Gets the service collection.
    /// </summary>
        public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the configuration instance.
    /// </summary>
        public IConfiguration Config { get; }

    /// <summary>
    /// Gets the TypeLoader instance.
    /// </summary>
        public TypeLoader TypeLoader { get; }

    /// <summary>
    /// Gets the logger factory used by the builder.
    /// </summary>
        public ILoggerFactory BuilderLoggerFactory { get; }

    /// <summary>
    /// Gets the hosting environment used by the builder.
    /// </summary>
        public IHostingEnvironment BuilderHostingEnvironment { get; }

    /// <summary>
    /// Gets the <see cref="IProfiler"/> instance associated with this build stub.
    /// </summary>
        public IProfiler Profiler { get; }

    /// <summary>
    /// Gets the <see cref="AppCaches"/> instance used by this <see cref="UmbracoBuildStub"/>.
    /// </summary>
        public AppCaches AppCaches { get; }

    /// <summary>
    /// Creates or retrieves a collection builder of the specified type.
    /// </summary>
    /// <returns>The collection builder instance of type <typeparamref name="TBuilder"/>.</returns>
        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder : ICollectionBuilder => default;

    /// <summary>
    /// Represents a stub implementation of a build method for testing purposes in the Umbraco build process.
    /// </summary>
        public void Build()
        {
        }
    }

    private class StubNotificationHandler
        : INotificationHandler<ContentPublishedNotification>,
            INotificationAsyncHandler<ContentPublishedNotification>
    {
    /// <summary>
    /// Handles the ContentPublishedNotification asynchronously.
    /// </summary>
    /// <param name="notification">The content published notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
        public Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Handles the ContentPublishedNotification.
    /// </summary>
    /// <param name="notification">The content published notification instance.</param>
        public void Handle(ContentPublishedNotification notification)
        {
        }
    }
}
