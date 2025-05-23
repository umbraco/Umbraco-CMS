using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypesSelector : ISubTypesSelector
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumerable<ISubTypesHandler> _subTypeHandlers;
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    [Obsolete("The settings parameter is not required anymore, use the other constructor instead. Scheduled for removal in Umbraco 17.")]
    public SubTypesSelector(
        IOptions<GlobalSettings> settings,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<ISubTypesHandler> subTypeHandlers,
        IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _subTypeHandlers = subTypeHandlers;
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    public SubTypesSelector(
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<ISubTypesHandler> subTypeHandlers,
        IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _subTypeHandlers = subTypeHandlers;
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    public IEnumerable<Type> SubTypes(Type type)
    {
        var backOfficePath = _hostingEnvironment.GetBackOfficePath();
        var swaggerPath = $"{backOfficePath}/swagger";

        if (_httpContextAccessor.HttpContext?.Request.Path.StartsWithSegments(swaggerPath) ?? false)
        {
            // Split the path into segments
            var segments = _httpContextAccessor.HttpContext.Request.Path.Value!
                .Substring(swaggerPath.Length)
                .TrimStart(Constants.CharArrays.ForwardSlash)
                .Split(Constants.CharArrays.ForwardSlash);

            // Extract the document name from the path
            var documentName = segments[0];

            // Find the first handler that can handle the type / document name combination
            ISubTypesHandler? handler = _subTypeHandlers.FirstOrDefault(h => h.CanHandle(type, documentName));
            if (handler != null)
            {
                return handler.Handle(type);
            }
        }

        // Default implementation to maintain backwards compatibility
        return _umbracoJsonTypeInfoResolver.FindSubTypes(type);
    }
}
