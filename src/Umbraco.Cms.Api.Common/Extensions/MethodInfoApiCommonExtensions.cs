using System.Reflection;
using Asp.Versioning;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="MethodInfo"/> to work with API-related attributes.
/// </summary>
public static class MethodInfoApiCommonExtensions
{
    /// <summary>
    ///     Gets the API version values from <see cref="MapToApiVersionAttribute"/> applied to the method.
    /// </summary>
    /// <param name="methodInfo">The method info to inspect.</param>
    /// <returns>A pipe-separated string of API version values.</returns>
    public static string GetMapToApiVersionAttributeValue(this MethodInfo methodInfo)
    {
        MapToApiVersionAttribute[] mapToApis = methodInfo.GetCustomAttributes(typeof(MapToApiVersionAttribute), inherit: true).Cast<MapToApiVersionAttribute>().ToArray();

        return string.Join("|", mapToApis.SelectMany(x => x.Versions));
    }

    /// <summary>
    ///     Gets the API name from <see cref="MapToApiAttribute"/> applied to the method's declaring type.
    /// </summary>
    /// <param name="methodInfo">The method info to inspect.</param>
    /// <returns>The API name if the attribute is present; otherwise, <c>null</c>.</returns>
    public static string? GetMapToApiAttributeValue(this MethodInfo methodInfo)
    {
        MapToApiAttribute[] mapToApis = (methodInfo.DeclaringType?.GetCustomAttributes(typeof(MapToApiAttribute), inherit: true) ?? Array.Empty<object>()).Cast<MapToApiAttribute>().ToArray();

        return mapToApis.SingleOrDefault()?.ApiName;
    }

    /// <summary>
    ///     Determines whether the method's declaring type has a <see cref="MapToApiAttribute"/> with the specified API name.
    /// </summary>
    /// <param name="methodInfo">The method info to inspect.</param>
    /// <param name="apiName">The API name to check for.</param>
    /// <returns>
    ///     <c>true</c> if the attribute is present and matches the specified API name,
    ///     or if the attribute is not present and the API name matches the default API name; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasMapToApiAttribute(this MethodInfo methodInfo, string apiName)
    {
        var value = methodInfo.GetMapToApiAttributeValue();

        return value == apiName
               || (value is null && apiName == DefaultApiConfiguration.ApiName);
    }
}
