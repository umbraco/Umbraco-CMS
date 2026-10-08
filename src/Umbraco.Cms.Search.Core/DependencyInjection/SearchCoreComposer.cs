using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Search.Core.DependencyInjection;

/// <summary>
/// Registers the core Umbraco Search services.
/// </summary>
public sealed class SearchCoreComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddSearchCore();
}
