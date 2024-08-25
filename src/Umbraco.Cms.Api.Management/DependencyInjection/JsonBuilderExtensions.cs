using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class JsonBuilderExtensions
{
    internal static IUmbracoBuilder AddJson(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddTransient<IJsonPatchService, JsonPatchService>();

        return builder;
    }
}
