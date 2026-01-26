using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.NewsDashboard;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class NewsDashboardBuilderExtensions
{
    internal static IUmbracoBuilder AddNewsDashboard(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<INewsCacheDurationProvider, NewsCacheDurationProvider>();
        builder.Services.AddSingleton<INewsDashboardService, NewsDashboardService>();

        return builder;
    }
}

