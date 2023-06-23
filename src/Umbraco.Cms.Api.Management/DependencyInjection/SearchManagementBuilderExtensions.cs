using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Extensions;
using Umbraco.Search;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Indexing;
using Umbraco.Search.Services;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class SearchManagementBuilderExtensions
{
    internal static IUmbracoBuilder AddSearchManagement(this IUmbracoBuilder builder)
    {
        // Add examine service
        builder.Services.AddTransient<IIndexingRebuilderService, IndexingRebuilderService>();

        // Add factories
        builder.Services.AddTransient<IIndexViewModelFactory, IndexViewModelFactory>();
        return builder;
    }
}
