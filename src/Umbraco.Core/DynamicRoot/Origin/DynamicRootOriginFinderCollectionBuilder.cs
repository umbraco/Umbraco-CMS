using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     A collection builder for <see cref="DynamicRootOriginFinderCollection"/> that allows ordered registration of <see cref="IDynamicRootOriginFinder"/> implementations.
/// </summary>
public class DynamicRootOriginFinderCollectionBuilder : OrderedCollectionBuilderBase<DynamicRootOriginFinderCollectionBuilder, DynamicRootOriginFinderCollection, IDynamicRootOriginFinder>
{
    /// <inheritdoc/>
    protected override DynamicRootOriginFinderCollectionBuilder This => this;
}
