using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
///     Represents the Umbraco builder used to configure services and dependencies during application startup.
/// </summary>
public interface IUmbracoBuilder
{
    /// <summary>
    ///     Gets the <see cref="IServiceCollection" /> where services are configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    ///     Gets the <see cref="IConfiguration" /> containing the application configuration properties.
    /// </summary>
    IConfiguration Config { get; }

    /// <summary>
    ///     Gets the <see cref="Composing.TypeLoader" /> used for type discovery and loading.
    /// </summary>
    TypeLoader TypeLoader { get; }

    /// <summary>
    ///     A Logger factory created specifically for the <see cref="IUmbracoBuilder" />. This is NOT the same
    ///     instance that will be resolved from DI. Use only if required during configuration.
    /// </summary>
    ILoggerFactory BuilderLoggerFactory { get; }

    /// <summary>
    ///     Gets the <see cref="IProfiler" /> used for performance profiling.
    /// </summary>
    IProfiler Profiler { get; }

    /// <summary>
    ///     Gets the <see cref="Cache.AppCaches" /> containing the application caches.
    /// </summary>
    AppCaches AppCaches { get; }

    /// <summary>
    ///     Gets a collection builder and registers the collection.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
    /// <returns>The collection builder instance.</returns>
    TBuilder WithCollectionBuilder<TBuilder>()
        where TBuilder : ICollectionBuilder;

    /// <summary>
    ///     Builds the Umbraco services by registering all configured collection builders with the service collection.
    /// </summary>
    void Build();
}
