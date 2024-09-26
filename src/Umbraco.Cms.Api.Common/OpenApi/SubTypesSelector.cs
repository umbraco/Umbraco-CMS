using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypesSelector : ISubTypesSelector
{
    private readonly IOptions<GlobalSettings> _settings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumerable<ISubTypesHandler> _subTypeHandlers;
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public SubTypesSelector(
        IOptions<GlobalSettings> settings,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<ISubTypesHandler> subTypeHandlers,
        IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _settings = settings;
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _subTypeHandlers = subTypeHandlers;
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    public IEnumerable<Type> SubTypes(Type type)
    {
        var backOfficePath =  _settings.Value.GetBackOfficePath(_hostingEnvironment);
        if (_httpContextAccessor.HttpContext?.Request.Path.StartsWithSegments($"{backOfficePath}/swagger") ?? false)
        {
            // Split the path into segments
            var segments = _httpContextAccessor.HttpContext.Request.Path.Value!.TrimStart($"{backOfficePath}/swagger/").Split('/');

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
