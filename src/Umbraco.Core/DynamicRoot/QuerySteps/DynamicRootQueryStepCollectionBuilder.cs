using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     A collection builder for <see cref="DynamicRootQueryStepCollection"/> that allows ordered registration of <see cref="IDynamicRootQueryStep"/> implementations.
/// </summary>
public class DynamicRootQueryStepCollectionBuilder : OrderedCollectionBuilderBase<DynamicRootQueryStepCollectionBuilder, DynamicRootQueryStepCollection, IDynamicRootQueryStep>
{
    /// <inheritdoc/>
    protected override DynamicRootQueryStepCollectionBuilder This => this;
}
