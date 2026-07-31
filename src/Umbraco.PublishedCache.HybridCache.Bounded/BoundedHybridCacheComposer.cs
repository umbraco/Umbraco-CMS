using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Bounded;

/// <summary>
///     Activates the bounded L0 published-content cache simply by being present: installing this package is
///     enough to opt in, with the bound itself controlled by the <c>MaximumLocalCacheItems</c> settings.
/// </summary>
public sealed class BoundedHybridCacheComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder) => builder.AddBoundedHybridCache();
}
