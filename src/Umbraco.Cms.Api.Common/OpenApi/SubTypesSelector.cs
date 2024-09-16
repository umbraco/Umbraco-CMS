using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypesSelector(
    IOptions<GlobalSettings> settings,
    IHostingEnvironment hostingEnvironment,
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<ISubTypesHandler> subTypeHandlers,
    IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    : OpenApiSelectorBase(settings, hostingEnvironment, httpContextAccessor), ISubTypesSelector
{
    public IEnumerable<Type> SubTypes(Type type)
    {
        var documentName = ResolveOpenApiDocumentName();
        if (!string.IsNullOrEmpty(documentName))
        {
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
