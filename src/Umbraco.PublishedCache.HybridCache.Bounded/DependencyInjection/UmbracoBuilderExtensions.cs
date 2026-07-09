using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.HybridCache.Bounded;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> that enable the bounded, scan-resistant L0
///     published-content cache.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Registers the bounded, scan-resistant (W-TinyLFU) implementation of the in-process (L0)
    ///     published-content cache. Once registered, configuring
    ///     <c>Umbraco:CMS:Cache:Entry:Document:MaximumLocalCacheItems</c> /
    ///     <c>...Media.MaximumLocalCacheItems</c> bounds the corresponding cache; leaving them unset keeps the
    ///     unbounded behaviour.
    /// </summary>
    public static IUmbracoBuilder AddBoundedHybridCache(this IUmbracoBuilder builder)
    {
        builder.Services
            .TryAddSingleton<IBoundedConvertedPublishedContentCacheFactory, BoundedConvertedPublishedContentCacheFactory>();
        return builder;
    }
}
