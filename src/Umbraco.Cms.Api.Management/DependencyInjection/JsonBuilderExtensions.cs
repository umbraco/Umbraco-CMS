using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring JSON serialization in the Umbraco CMS Management API.
/// </summary>
public static class JsonBuilderExtensions
{
    internal static IUmbracoBuilder AddJson(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddTransient<IJsonPatchService, JsonPatchService>();

        return builder;
    }
}
