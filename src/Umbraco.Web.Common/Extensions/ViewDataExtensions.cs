using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Extensions;

public static class ViewDataExtensions
{
    public const string TokenUmbracoPath = "UmbracoPath";
    public const string TokenInstallApiBaseUrl = "InstallApiBaseUrl";
    public const string TokenUmbracoBaseFolder = "UmbracoBaseFolder";
    public const string TokenUmbracoVersion = "UmbracoVersion";
    public const string TokenExternalSignInError = "ExternalSignInError";
    public const string TokenPasswordResetCode = "PasswordResetCode";
    public const string TokenTwoFactorRequired = "TwoFactorRequired";

    public static bool FromTempData(this ViewDataDictionary viewData, ITempDataDictionary tempData, string token)
    {
        if (tempData[token] == null)
        {
            return false;
        }

        viewData[token] = tempData[token];
        return true;
    }

    /// <summary>
    ///     Copies data from a request cookie to view data and then clears the cookie in the response
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         This is similar to TempData but in some cases we cannot use TempData which relies on the temp data provider and
    ///         session.
    ///         The cookie value can either be a simple string value
    ///     </para>
    /// </remarks>
    public static bool FromBase64CookieData<T>(
        this ViewDataDictionary viewData,
        HttpContext? httpContext,
        string cookieName,
        IJsonSerializer serializer)
    {
        var hasCookie = httpContext?.Request.Cookies.ContainsKey(cookieName) ?? false;
        if (!hasCookie)
        {
            return false;
        }

        // get the cookie value
        if (httpContext is null || !httpContext.Request.Cookies.TryGetValue(cookieName, out var cookieVal))
        {
            return false;
        }

        // ensure the cookie is expired (must be done after reading the value)
        httpContext.Response.Cookies.Delete(cookieName);

        if (cookieVal.IsNullOrWhiteSpace())
        {
            return false;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(WebUtility.UrlDecode(cookieVal)!));

            // deserialize to T and store in viewdata
            viewData[cookieName] = serializer.Deserialize<T>(decoded);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string? GetUmbracoPath(this ViewDataDictionary viewData) => (string?)viewData[TokenUmbracoPath];

    public static void SetUmbracoPath(this ViewDataDictionary viewData, string value) =>
        viewData[TokenUmbracoPath] = value;

    public static string? GetInstallApiBaseUrl(this ViewDataDictionary viewData) =>
        (string?)viewData[TokenInstallApiBaseUrl];

    public static void SetInstallApiBaseUrl(this ViewDataDictionary viewData, string? value) =>
        viewData[TokenInstallApiBaseUrl] = value;

    public static string? GetUmbracoBaseFolder(this ViewDataDictionary viewData) =>
        (string?)viewData[TokenUmbracoBaseFolder];

    public static void SetUmbracoBaseFolder(this ViewDataDictionary viewData, string value) =>
        viewData[TokenUmbracoBaseFolder] = value;

    public static void SetUmbracoVersion(this ViewDataDictionary viewData, SemVersion version) =>
        viewData[TokenUmbracoVersion] = version;

    public static SemVersion? GetUmbracoVersion(this ViewDataDictionary viewData) =>
        (SemVersion?)viewData[TokenUmbracoVersion];

    /// <summary>
    ///     Used by the back office login screen to get any registered external login provider errors
    /// </summary>
    /// <param name="viewData"></param>
    /// <returns></returns>
    public static BackOfficeExternalLoginProviderErrors?
        GetExternalSignInProviderErrors(this ViewDataDictionary viewData) =>
        (BackOfficeExternalLoginProviderErrors?)viewData[TokenExternalSignInError];

    /// <summary>
    ///     Used by the back office controller to register any external login provider errors
    /// </summary>
    /// <param name="viewData"></param>
    /// <param name="errors"></param>
    public static void SetExternalSignInProviderErrors(
        this ViewDataDictionary viewData,
        BackOfficeExternalLoginProviderErrors errors) => viewData[TokenExternalSignInError] = errors;

    public static string? GetPasswordResetCode(this ViewDataDictionary viewData) =>
        (string?)viewData[TokenPasswordResetCode];

    public static void SetPasswordResetCode(this ViewDataDictionary viewData, string value) =>
        viewData[TokenPasswordResetCode] = value;

    public static void SetTwoFactorProviderNames(this ViewDataDictionary viewData, IEnumerable<string> providerNames) =>
        viewData[TokenTwoFactorRequired] = providerNames;

    public static bool TryGetTwoFactorProviderNames(
        this ViewDataDictionary viewData,
        [MaybeNullWhen(false)] out IEnumerable<string> providerNames)
    {
        providerNames = viewData[TokenTwoFactorRequired] as IEnumerable<string>;
        return providerNames is not null;
    }
}
