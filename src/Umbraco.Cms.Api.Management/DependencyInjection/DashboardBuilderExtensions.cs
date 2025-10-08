using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.NewsDashboard;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class DashboardBuilderExtensions
{
    internal static IUmbracoBuilder AddDashboard(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<INewsDashboardService, NewsDashboardService>();

        return builder;
    }
}
