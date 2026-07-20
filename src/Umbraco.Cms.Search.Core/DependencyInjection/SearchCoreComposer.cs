using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Search.Core.DependencyInjection;

public sealed class SearchCoreComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddSearchCore();
}
