using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

public static class MethodInfoApiCommonExtensions
{

    public static string? GetMapToApiVersionAttributeValue(this MethodInfo methodInfo)
    {
        var mapToApis = methodInfo.GetCustomAttributes(typeof(MapToApiVersionAttribute), inherit: true).Cast<MapToApiVersionAttribute>().ToArray();

        return string.Join("|", mapToApis.SelectMany(x=>x.Versions));
    }
    public static string? GetMapToApiAttributeValue(this MethodInfo methodInfo)
    {
        var mapToApis = (methodInfo.DeclaringType?.GetCustomAttributes(typeof(MapToApiAttribute), inherit: true) ?? Array.Empty<object>()).Cast<MapToApiAttribute>().ToArray();

        return mapToApis.SingleOrDefault()?.ApiName;
    }
    public static bool HasMapToApiAttribute(this MethodInfo methodInfo, string apiName)
    {
        var value = methodInfo.GetMapToApiAttributeValue();

        return value == apiName
               || (value is null && apiName == DefaultApiConfiguration.ApiName);
    }
}
