using Microsoft.AspNetCore.Mvc.Abstractions;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

public static class ActionDescriptorApiCommonExtensions
{
    private static string? GetMapToApiAttributeValue(this ActionDescriptor actionDescriptor)
    {
        IEnumerable<MapToApiAttribute> mapToApiAttributes = actionDescriptor?.EndpointMetadata?.OfType<MapToApiAttribute>() ?? [];

        return mapToApiAttributes.SingleOrDefault()?.ApiName;
    }

    public static bool HasMapToApiAttribute(this ActionDescriptor actionDescriptor, string apiName)
    {
        var value = actionDescriptor.GetMapToApiAttributeValue();

        return value == apiName
               || (value is null && apiName == DefaultApiConfiguration.ApiName);
    }
}
