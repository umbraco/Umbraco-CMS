using Microsoft.AspNetCore.Mvc.Abstractions;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ActionDescriptor"/> to work with <see cref="MapToApiAttribute"/>.
/// </summary>
public static class ActionDescriptorApiCommonExtensions
{
    /// <summary>
    /// Determines whether the <see cref="ActionDescriptor"/> has a <see cref="MapToApiAttribute"/> with the specified API name.
    /// The check is made in runtime to support attributes added in runtime.
    /// </summary>
    /// <param name="actionDescriptor">The action descriptor to inspect.</param>
    /// <param name="apiName">The API name to check for.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="MapToApiAttribute"/> is present and matches the specified API name,
    /// or if the attribute is not present and the API name matches the default API name; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Endpoint metadata lists the attributes declared on the controller, then those declared on the action,
    /// then any added at runtime, for example by an application model convention.
    /// When it holds more than one <see cref="MapToApiAttribute"/>, the last one in that order wins,
    /// so an attribute added at runtime overrides one declared on the controller or action.
    /// </remarks>
    public static bool HasMapToApiAttribute(this ActionDescriptor actionDescriptor, string apiName)
    {
        var value = actionDescriptor.GetMapToApiAttributeValue();

        return value == apiName
               || (value is null && apiName == DefaultApiConfiguration.ApiName);
    }

    private static string? GetMapToApiAttributeValue(this ActionDescriptor actionDescriptor)
    {
        IEnumerable<MapToApiAttribute> mapToApiAttributes = actionDescriptor?.EndpointMetadata?.OfType<MapToApiAttribute>() ?? [];

        return mapToApiAttributes.LastOrDefault()?.ApiName;
    }
}
