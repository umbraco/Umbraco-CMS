using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Helpers;

/// <remark>
/// This class seems unused, but is used by implementors to configure the error flow for external login providers
/// so that they properly route towards our default error handling page with the correct parameters.
/// </remark>
public class OAuthOptionsHelper
{
    // https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2.1
    // we omit "state" and "error_uri" here as it hold no value in determining the message to display to the user
    private static readonly string[] _oathCallbackErrorParams = ["error", "error_description"];

    private readonly IOptions<SecuritySettings> _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthOptionsHelper"/> class.
    /// </summary>
    /// <param name="securitySettings">The security settings options.</param>
    public OAuthOptionsHelper(IOptions<SecuritySettings> securitySettings) => _securitySettings = securitySettings;

    /// <summary>
    /// Applies SetUmbracoRedirectWithFilteredParams to both OnAccessDenied and OnRemoteFailure
    /// on the OAuthOptions so Umbraco can do its best to nicely display the error messages
    /// that are passed back from the external login provider on failure.
    /// </summary>
    public T SetDefaultErrorEventHandling<T>(T oAuthOptions, string providerFriendlyName) where T : OAuthOptions
    {
        oAuthOptions.Events.OnAccessDenied =
            context => HandleResponseWithDefaultUmbracoRedirect(context, providerFriendlyName, "OnAccessDenied");
        oAuthOptions.Events.OnRemoteFailure =
            context => HandleResponseWithDefaultUmbracoRedirect(context, providerFriendlyName, "OnRemoteFailure");

        return oAuthOptions;
    }

    private Task HandleResponseWithDefaultUmbracoRedirect(HandleRequestContext<RemoteAuthenticationOptions> context, string providerFriendlyName, string eventName)
    {
        SetUmbracoRedirectWithFilteredParams(context, providerFriendlyName, eventName)
            .HandleResponse();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the context to redirect to the <see cref="SecuritySettings.AuthorizeCallbackErrorPathName"/> path, forwarding the
    /// <c>error</c> and <c>error_description</c> parameters the external login provider sent to the callback, read from the
    /// query string or, when absent there, from a form post.
    /// </summary>
    public T SetUmbracoRedirectWithFilteredParams<T>(T context, string providerFriendlyName, string eventName)
        where T : HandleRequestContext<RemoteAuthenticationOptions>
    {
        var callbackPath =_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName;

        callbackPath = callbackPath.AppendQueryStringToUrl("flow=external-login")
            .AppendQueryStringToUrl($"provider={Uri.EscapeDataString(providerFriendlyName)}")
            .AppendQueryStringToUrl($"callback-event={Uri.EscapeDataString(eventName)}");

        // Providers that answer with response_mode=form_post (the OpenID Connect handler's default) send the error in the body.
        IFormCollection? form = context.Request.GetFormOrNull();

        foreach (var oathCallbackErrorParam in _oathCallbackErrorParams)
        {
            if (context.Request.Query.TryGetValue(oathCallbackErrorParam, out StringValues paramValue)
                || (form is not null && form.TryGetValue(oathCallbackErrorParam, out paramValue)))
            {
                callbackPath = callbackPath.AppendQueryStringToUrl($"{oathCallbackErrorParam}={Uri.EscapeDataString(paramValue.ToString())}");
            }
        }

        context.Response.Redirect(callbackPath);
        return context;
    }
}
