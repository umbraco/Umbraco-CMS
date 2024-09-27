using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public abstract class OpenApiSelectorBase(
    IOptions<GlobalSettings> settings,
    IHostingEnvironment hostingEnvironment,
    IHttpContextAccessor httpContextAccessor)
{
    protected string ResolveOpenApiDocumentName()
    {
        var backOfficePath = settings.Value.GetBackOfficePath(hostingEnvironment);
        if (httpContextAccessor.HttpContext?.Request.Path.StartsWithSegments($"{backOfficePath}/swagger/") ?? false)
        {
            // Split the path into segments
            var segments = httpContextAccessor.HttpContext.Request.Path.Value!.TrimStart($"{backOfficePath}/swagger/").Split('/');

            // Extract the document name from the path
            return segments[0];
        }
        return string.Empty;
    }
}
