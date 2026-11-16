using System.Reflection;
using Asp.Versioning;

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
}
