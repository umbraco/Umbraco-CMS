﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypeSelector(IOptions<GlobalSettings> settings,
    IHostingEnvironment hostingEnvironment,
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<ISubTypeHandler> subTypeHandlers,
    IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver) : ISubTypeSelector
{
    public IEnumerable<Type> SubTypes(Type type)
    {
        var backOfficePath =  settings.Value.GetBackOfficePath(hostingEnvironment);
        if (httpContextAccessor.HttpContext?.Request.Path.StartsWithSegments($"{backOfficePath}/swagger/") ?? false)
        {
            var segments = httpContextAccessor.HttpContext.Request.Path.Value!.TrimStart('/').Split('/');
            if (segments.Length >= 3)
            {
                // Extract the document name from the path
                var documentName = segments[2];

                // Find the first handler that can handle the type / document name combination
                ISubTypeHandler? handler = subTypeHandlers.FirstOrDefault(h => h.CanHandle(type, documentName));
                if (handler != null)
                {
                    return handler.Handle(type);
                }
            }
        }

        // Default implementation to maintain backwards compatibility
        return umbracoJsonTypeInfoResolver.FindSubTypes(type);
    }
}
