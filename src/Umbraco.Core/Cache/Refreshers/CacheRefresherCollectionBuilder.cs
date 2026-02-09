using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Builds a <see cref="CacheRefresherCollection" /> from registered <see cref="ICacheRefresher" /> instances.
/// </summary>
/// <remarks>
///     Use this builder to register custom cache refreshers during application composition.
/// </remarks>
public class CacheRefresherCollectionBuilder : LazyCollectionBuilderBase<CacheRefresherCollectionBuilder,
    CacheRefresherCollection, ICacheRefresher>
{
    /// <inheritdoc />
    protected override CacheRefresherCollectionBuilder This => this;
}
