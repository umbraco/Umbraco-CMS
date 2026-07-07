using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.BackOffice.DependencyInjection;

public sealed class BackOfficeSearchComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Temporary escape hatch: revert backoffice search to the legacy Examine based implementations.
        SearchSettings? searchSettings = builder.Config
            .GetSection(Umbraco.Cms.Core.Constants.Configuration.ConfigSearch)
            .Get<SearchSettings>();
        if (searchSettings?.UseLegacySearchServices is true)
        {
            return;
        }

        builder
            .AddSearchCore()
            .AddBackOfficeSearch();
    }
}
