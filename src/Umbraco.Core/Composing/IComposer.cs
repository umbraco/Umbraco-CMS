using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Represents a composer.
/// </summary>
public interface IComposer : IDiscoverable
{
    /// <summary>
    ///     Compose.
    /// </summary>
    void Compose(IUmbracoBuilder builder);
}
