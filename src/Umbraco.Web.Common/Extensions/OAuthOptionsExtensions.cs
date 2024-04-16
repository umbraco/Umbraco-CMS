using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Extensions;

public static class OAuthOptionsExtensions
{
    // https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2.1
    // we omit "state" and "error_uri" here as it hold no value in determining the message to display to the user
    private static IReadOnlyCollection<string> oathCallbackErrorParams = new string[] { "error", "error_description" };

    /// <summary>
    /// Applies SetUmbracoRedirectWithFilteredParams to both OnAccessDenied and OnRemoteFailure
    /// on the OAuthOptions so Umbraco can do its best to nicely display the error messages
    /// that are passed back from the external login provider on failure.
    /// </summary>
    public static T SetDefaultErrorEventHandling<T>(this T oAuthOptions, string providerFriendlyName) where T : OAuthOptions
    {
        oAuthOptions.Events.OnAccessDenied =
            context => HandleResponseWithDefaultUmbracoRedirect(context, providerFriendlyName, "OnAccessDenied");
        oAuthOptions.Events.OnRemoteFailure =
            context => HandleResponseWithDefaultUmbracoRedirect(context, providerFriendlyName, "OnRemoteFailure");

        return oAuthOptions;
    }

    private static Task HandleResponseWithDefaultUmbracoRedirect(HandleRequestContext<RemoteAuthenticationOptions> context, string providerFriendlyName, string eventName)
    {
        context.SetUmbracoRedirectWithFilteredParams(providerFriendlyName,eventName)
            .HandleResponse();

        return Task.FromResult(0);
    }

    /// <summary>
    /// Sets the context to redirect to the <see cref="SecuritySettings.AuthorizeCallbackErrorPathName"/> path with all parameters, except state, that are passed to the initial server callback configured for the configured external login provider
    /// </summary>
    public static T SetUmbracoRedirectWithFilteredParams<T>(this T context, string providerFriendlyName, string eventName)
        where T : HandleRequestContext<RemoteAuthenticationOptions>
    {
        var callbackPath = StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>().Value
            .AuthorizeCallbackErrorPathName;

        callbackPath = callbackPath.AppendQueryStringToUrl("flow=external-login")
        .AppendQueryStringToUrl($"provider={providerFriendlyName}")
        .AppendQueryStringToUrl($"callback-event={eventName}");

        foreach (var oathCallbackErrorParam in oathCallbackErrorParams)
        {
            if (context.Request.Query.ContainsKey(oathCallbackErrorParam))
            {
                callbackPath = callbackPath.AppendQueryStringToUrl($"{oathCallbackErrorParam}={context.Request.Query[oathCallbackErrorParam]}");
            }
        }

        context.Response.Redirect(callbackPath);
        return context;
    }

    /// <summary>
    /// Sets the callbackPath for the RemoteAuthenticationOptions based on the configured Umbraco path and the path supplied.
    /// By default this will result in "/umbraco/your-supplied-path".
    /// </summary>
    /// <param name="options">The options object to set the path on.</param>
    /// <param name="path">The path that should go after the umbraco path, will add a leading slash if it's missing.</param>
    /// <returns></returns>
    public static RemoteAuthenticationOptions SetUmbracoBasedCallbackPath(this RemoteAuthenticationOptions options, string path)
    {
        var umbracoCallbackPath = StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>().Value
            .AuthorizeCallbackPathName;

        options.CallbackPath = umbracoCallbackPath + path.EnsureStartsWith("/");
        return options;
    }
}
