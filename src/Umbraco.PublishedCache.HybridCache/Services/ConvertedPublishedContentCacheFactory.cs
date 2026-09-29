using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     The default <see cref="IConvertedPublishedContentCacheFactory" />: creates an unbounded cache unless a
///     maximum is configured and a bounded provider (the opt-in bounded cache package) is registered.
/// </summary>
internal sealed class ConvertedPublishedContentCacheFactory : IConvertedPublishedContentCacheFactory
{
    private readonly IBoundedConvertedPublishedContentCacheFactory? _boundedFactory;
    private readonly ILogger<ConvertedPublishedContentCacheFactory> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConvertedPublishedContentCacheFactory" /> class.
    /// </summary>
    /// <param name="boundedFactory">
    ///     The bounded cache provider, or <c>null</c> when the opt-in bounded cache package is not installed
    ///     (in which case a configured maximum falls back to an unbounded cache).
    /// </param>
    /// <param name="logger">The logger.</param>
    public ConvertedPublishedContentCacheFactory(
        IBoundedConvertedPublishedContentCacheFactory? boundedFactory,
        ILogger<ConvertedPublishedContentCacheFactory> logger)
    {
        _boundedFactory = boundedFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public IConvertedPublishedContentCache<TKey> Create<TKey>(int? maximumItems, string cacheName)
        where TKey : notnull
    {
        if (maximumItems is null)
        {
            _logger.LogInformation("{CacheName}: using an unbounded in-memory (L0) cache.", cacheName);
            return new UnboundedConvertedPublishedContentCache<TKey>();
        }

        if (_boundedFactory is null)
        {
            // A maximum is configured but nothing can honour it: fall back to unbounded rather than fail to
            // boot. The bounded behaviour requires the opt-in bounded cache package to be installed.
            _logger.LogWarning(
                "{CacheName}: a maximum local cache item count ({MaximumItems}) is configured, but the bounded published-content cache package is not installed. The L0 cache will remain unbounded. Install the {PackageName} package to enable a bounded, scan-resistant cache.",
                cacheName,
                maximumItems.Value,
                "Umbraco.Cms.PublishedCache.HybridCache.Bounded");

            return new UnboundedConvertedPublishedContentCache<TKey>();
        }

        _logger.LogInformation(
            "{CacheName}: using a bounded, scan-resistant in-memory (L0) cache with a maximum of {MaximumItems} items.",
            cacheName,
            maximumItems.Value);

        return _boundedFactory.CreateBounded<TKey>(maximumItems.Value);
    }
}
