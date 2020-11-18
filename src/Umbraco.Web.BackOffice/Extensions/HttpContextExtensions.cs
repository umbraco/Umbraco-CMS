using Microsoft.AspNetCore.Http;
using Umbraco.Core.Security;

namespace Umbraco.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetExternalLoginProviderErrors(this HttpContext httpContext, BackOfficeExternalLoginProviderErrors errors)
            => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] = errors;

        public static BackOfficeExternalLoginProviderErrors GetExternalLoginProviderErrors(this HttpContext httpContext)
            => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] as BackOfficeExternalLoginProviderErrors;
    }
}
