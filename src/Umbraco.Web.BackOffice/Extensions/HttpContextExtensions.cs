using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

public static class HttpContextExtensions
{
    private const string PasswordResetFlowSessionKey = nameof(PasswordResetFlowSessionKey);

    public static void SetExternalLoginProviderErrors(this HttpContext httpContext, BackOfficeExternalLoginProviderErrors errors)
        => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] = errors;

    public static BackOfficeExternalLoginProviderErrors? GetExternalLoginProviderErrors(this HttpContext httpContext)
        => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] as BackOfficeExternalLoginProviderErrors;

    internal static void StartPasswordResetFlowSession(this HttpContext httpContext, int userId)
        => httpContext.Session.SetInt32(PasswordResetFlowSessionKey, userId);

    internal static void EndPasswordResetFlowSession(this HttpContext httpContext)
        => httpContext.Session.Remove(PasswordResetFlowSessionKey);

    internal static bool HasActivePasswordResetFlowSession(this HttpContext httpContext, int userId)
        => httpContext.Session.GetInt32(PasswordResetFlowSessionKey) == userId;
}
