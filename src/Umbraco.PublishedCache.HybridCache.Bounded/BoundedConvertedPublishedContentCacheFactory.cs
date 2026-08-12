using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Bounded;

/// <summary>
///     Supplies the BitFaster-backed <see cref="BoundedConvertedPublishedContentCache{TKey,TValue}" />.
/// </summary>
internal sealed class BoundedConvertedPublishedContentCacheFactory : IBoundedConvertedPublishedContentCacheFactory
{
    /// <inheritdoc />
    public IConvertedPublishedContentCache<TKey, TValue> CreateBounded<TKey, TValue>(int maximumItems)
        where TKey : notnull
        where TValue : class, IPublishedElement
        => new BoundedConvertedPublishedContentCache<TKey, TValue>(maximumItems);
}
