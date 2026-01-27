using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Common.Filters;

namespace Umbraco.Cms.Api.Common.Json;

/// <summary>
///     Extension methods for <see cref="HttpContext"/> related to JSON serialization.
/// </summary>
public static class HttpContextJsonExtensions
{
    /// <summary>
    ///     Gets the named JSON options configuration for the current endpoint.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The JSON options name if specified via <see cref="JsonOptionsNameAttribute"/>; otherwise, <c>null</c>.</returns>
    public static string? CurrentJsonOptionsName(this HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<JsonOptionsNameAttribute>()?.JsonOptionsName;
}
