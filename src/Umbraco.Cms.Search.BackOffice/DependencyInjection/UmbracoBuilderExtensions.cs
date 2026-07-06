using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.BackOffice.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.BackOffice.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddBackOfficeSearch(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IIndexedEntitySearchService, IndexedEntitySearchService>();
        builder.Services.AddUnique<IContentSearchService, ContentSearchService>();
        builder.Services.AddUnique<IMediaSearchService, MediaSearchService>();

        return builder;
    }
}
