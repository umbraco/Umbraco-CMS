using System.Reflection;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

public static class MethodInfoApiCommonExtensions
{
    public static bool HasMapToApiAttribute(this MethodInfo methodInfo, string apiName)
    {
        var mapToApis = (methodInfo.DeclaringType?.GetCustomAttributes(typeof(MapToApiAttribute), inherit: true) ?? Array.Empty<object>()).Cast<MapToApiAttribute>().ToArray();

        return mapToApis.Any(x => x.ApiName == apiName)
               || (mapToApis.Any() == false && apiName == DefaultApiConfiguration.ApiName);
    }
}
