using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Bounded;

/// <summary>
///     Supplies the BitFaster-backed <see cref="BoundedConvertedPublishedContentCache{TKey}" />.
/// </summary>
internal sealed class BoundedConvertedPublishedContentCacheFactory : IBoundedConvertedPublishedContentCacheFactory
{
    /// <inheritdoc />
    public IConvertedPublishedContentCache<TKey> CreateBounded<TKey>(int maximumItems)
        where TKey : notnull
        => new BoundedConvertedPublishedContentCache<TKey>(maximumItems);
}
