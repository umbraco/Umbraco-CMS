using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Services;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class ServicesBuilderExtensions
{
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IJsonPatchService, JsonPatchService>();
        return builder;
    }
}
