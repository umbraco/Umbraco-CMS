using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Web.Common.Extensions;

public static class OAuthOptionsExtensions
{
    /// <summary>
    /// Applies SetUmbracoRedirectWithFilteredParams to both OnAccessDenied and OnRemoteFailure
    /// on the OAuthOptions so Umbraco can do its best to nicely display the error messages
    /// that are passed back from the external login provider on failure.
    /// </summary>
    /// <param name="oAuthOptions"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T SetDefaultErrorEventHandling<T>(this T oAuthOptions) where T : OAuthOptions
    {
        oAuthOptions.Events.OnAccessDenied = HandleResponseWithDefaultUmbracoRedirect;
        oAuthOptions.Events.OnRemoteFailure = HandleResponseWithDefaultUmbracoRedirect;

        return oAuthOptions;
    }

    private static Task HandleResponseWithDefaultUmbracoRedirect(HandleRequestContext<RemoteAuthenticationOptions> context)
    {
        context.SetUmbracoRedirectWithFilteredParams()
            .HandleResponse();

        return Task.FromResult(0);
    }

    /// <summary>
    /// Sets the context to redirect to the <see cref="SecuritySettings.AuthorizeCallbackErrorPathName"/> path with all parameters, except state, that are passed to the initial server callback configured for the configured external login provider
    /// </summary>
    /// <returns></returns>
    public static T SetUmbracoRedirectWithFilteredParams<T>(this T context)
        where T : HandleRequestContext<RemoteAuthenticationOptions>
    {
        var callbackPath = StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>().Value
            .AuthorizeCallbackErrorPathName;
        context.Response.Redirect($"{callbackPath}?" + string.Join(
            '&',
            context.Request.Query.Where(q =>
                    q.Key.Equals("state", StringComparison.InvariantCultureIgnoreCase) == false)
                .Select(q => q.Key + "=" + q.Value)));

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
        var umbracoDallbackPath = StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>().Value
            .AuthorizeCallbackPathName;
        if (path.StartsWith('/') == false)
        {
            path = "/" + path;
        }

        options.CallbackPath = umbracoDallbackPath + path;
        return options;
    }
}
