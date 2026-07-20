using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.BackOffice.DependencyInjection;

public sealed class BackOfficeSearchComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder
            .AddSearchCore()
            .AddBackOfficeSearch();
}
