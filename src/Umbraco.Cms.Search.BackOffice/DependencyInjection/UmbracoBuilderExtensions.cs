using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.BackOffice.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.BackOffice.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Enables Umbraco Search as the engine for the backoffice search services.
    /// </summary>
    /// <remarks>
    /// This method is idempotent - calling it multiple times has no effect after the first call.
    /// </remarks>
    public static IUmbracoBuilder AddBackOfficeSearch(this IUmbracoBuilder builder)
    {
        // Idempotency check - safe to call multiple times.
        if (builder.Services.Any(s => s.ServiceType == typeof(AddBackOfficeSearchMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<AddBackOfficeSearchMarker>();

        builder.Services.AddUnique<IIndexedEntitySearchService, IndexedEntitySearchService>();
        builder.Services.AddUnique<IContentSearchService, ContentSearchService>();
        builder.Services.AddUnique<IMediaSearchService, MediaSearchService>();

        return builder;
    }

    private sealed class AddBackOfficeSearchMarker
    {
    }
}
