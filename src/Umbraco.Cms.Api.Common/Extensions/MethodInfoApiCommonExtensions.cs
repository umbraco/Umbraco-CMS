using System.Reflection;
using Asp.Versioning;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

public static class MethodInfoApiCommonExtensions
{

    public static string? GetMapToApiVersionAttributeValue(this MethodInfo methodInfo)
    {
        MapToApiVersionAttribute[] mapToApis = methodInfo.GetCustomAttributes(typeof(MapToApiVersionAttribute), inherit: true).Cast<MapToApiVersionAttribute>().ToArray();

        return string.Join("|", mapToApis.SelectMany(x=>x.Versions));
    }

    public static MapToApiAttribute? GetMapToApiAttribute(this MethodInfo methodInfo) => methodInfo.DeclaringType?.GetCustomAttributes<MapToApiAttribute>(inherit: true).SingleOrDefault();

    [Obsolete("Use GetMapToApiAttribute instead and get the value from .ApiName")]
    public static string? GetMapToApiAttributeValue(this MethodInfo methodInfo) => GetMapToApiAttribute(methodInfo)?.ApiName;

    [Obsolete("Use GetMapToApiAttribute instead.")]
    public static bool HasMapToApiAttribute(this MethodInfo methodInfo, string apiName)
    {
        MapToApiAttribute? mapToApiAttribute = GetMapToApiAttribute(methodInfo);

        if (mapToApiAttribute is null)
        {
            return apiName == DefaultApiConfiguration.ApiName;
        }

        return mapToApiAttribute.ApiName == apiName;
    }
}
