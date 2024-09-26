using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypesSelector(IOptions<GlobalSettings> settings,
    IHostingEnvironment hostingEnvironment,
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<ISubTypesHandler> subTypeHandlers,
    IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver) : ISubTypesSelector
{
    public IEnumerable<Type> SubTypes(Type type)
    {
        var backOfficePath =  settings.Value.GetBackOfficePath(hostingEnvironment);
        if (httpContextAccessor.HttpContext?.Request.Path.StartsWithSegments($"{backOfficePath}/swagger/") ?? false)
        {
            // Split the path into segments
            var segments = httpContextAccessor.HttpContext.Request.Path.Value!.TrimStart($"{backOfficePath}/swagger/").Split('/');

            // Extract the document name from the path
            var documentName = segments[0];

            // Find the first handler that can handle the type / document name combination
            ISubTypesHandler? handler = subTypeHandlers.FirstOrDefault(h => h.CanHandle(type, documentName));
            if (handler != null)
            {
                return handler.Handle(type);
            }
        }

        // Default implementation to maintain backwards compatibility
        return umbracoJsonTypeInfoResolver.FindSubTypes(type);
    }
}
