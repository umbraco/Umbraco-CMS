using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.BackOffice.DependencyInjection;

/// <summary>
/// Registers the search core and backoffice search services.
/// </summary>
public sealed class BackOfficeSearchComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder
            .AddSearchCore()
            .AddBackOfficeSearch();
}
