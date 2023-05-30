using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Common.Filters;

namespace Umbraco.Cms.Api.Common.Json;

public static class HttpContextJsonExtensions
{
    public static string? CurrentJsonOptionsName(this HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<JsonOptionsNameAttribute>()?.JsonOptionsName;
}
