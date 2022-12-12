using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Management.Filters;

namespace Umbraco.Cms.Api.Management.Json;

public static class HttpContextJsonExtensions
{
    public static string? CurrentJsonOptionsName(this HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<JsonOptionsNameAttribute>()?.JsonOptionsName;
}
