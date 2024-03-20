using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Represents a composer that can be used in a custom composer step.
/// </summary>
public interface ICustomComposer : IDiscoverable
{
    /// <summary>
    ///     Compose.
    /// </summary>
    void Compose(IUmbracoBuilder builder);
}
