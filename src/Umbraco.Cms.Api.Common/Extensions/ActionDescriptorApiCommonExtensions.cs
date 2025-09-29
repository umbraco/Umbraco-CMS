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
    /// Retrieves the <see cref="MapToApiAttribute.ApiName"/> value from the <see cref="ActionDescriptor"/>'s endpoint metadata, if present.
    /// </summary>
    /// <param name="actionDescriptor">The action descriptor to inspect.</param>
    /// <returns>
    /// The API name specified in the <see cref="MapToApiAttribute"/>, or <c>null</c> if the attribute is not present.
    /// </returns>
    private static string? GetMapToApiAttributeValue(this ActionDescriptor actionDescriptor)
    {
        IEnumerable<MapToApiAttribute> mapToApiAttributes = actionDescriptor?.EndpointMetadata?.OfType<MapToApiAttribute>() ?? [];

        return mapToApiAttributes.SingleOrDefault()?.ApiName;
    }

    /// <summary>
    /// Determines whether the <see cref="ActionDescriptor"/> has a <see cref="MapToApiAttribute"/> with the specified API name.
    /// </summary>
    /// <param name="actionDescriptor">The action descriptor to inspect.</param>
    /// <param name="apiName">The API name to check for.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="MapToApiAttribute"/> is present and matches the specified API name,
    /// or if the attribute is not present and the API name matches the default API name; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasMapToApiAttribute(this ActionDescriptor actionDescriptor, string apiName)
    {
        var value = actionDescriptor.GetMapToApiAttributeValue();

        return value == apiName
               || (value is null && apiName == DefaultApiConfiguration.ApiName);
    }
}
