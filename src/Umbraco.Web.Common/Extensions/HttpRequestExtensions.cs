using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Extensions
{
    public static class HttpRequestExtensions
    {
        internal static string ClientCulture(this HttpRequest request)
        {
            return request.Headers.TryGetValue("X-UMB-CULTURE", out var values) ? values[0] : null;
        }
    }
}
