using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Extension methods for registering JSON-related services.
/// </summary>
[Obsolete("JsonPatch.Net dependency and IJsonPatchService are being removed. Use the custom patch engine (DocumentPatcher) instead. Scheduled for removal in Umbraco 19.")]
public static class JsonBuilderExtensions
{
    /// <summary>
    /// Adds JSON-related services to the Umbraco builder.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The Umbraco builder.</returns>
    internal static IUmbracoBuilder AddJson(this IUmbracoBuilder builder)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services
            .AddTransient<IJsonPatchService, JsonPatchService>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }
}
