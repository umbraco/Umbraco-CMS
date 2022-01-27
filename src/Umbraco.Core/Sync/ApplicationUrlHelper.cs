using System;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A helper used to determine the current server umbraco application URL.
    /// </summary>
    public static class ApplicationUrlHelper
    {
        // because we cannot logger.Info<ApplicationUrlHelper> because type is static
        private static readonly Type TypeOfApplicationUrlHelper = typeof(ApplicationUrlHelper);

        /// <summary>
        /// Gets or sets a custom provider for the umbraco application URL.
        /// </summary>
        /// <remarks>
        /// <para>Receives the current request as a parameter, and it may be null. Must return a properly
        /// formatted URL with scheme and umbraco dir and no trailing slash eg "http://www.mysite.com/umbraco",
        /// or <c>null</c>. To be used in auto-load-balancing scenarios where the application URL is not
        /// in config files but is determined programmatically.</para>
        /// <para>Must be assigned before resolution is frozen.</para>
        /// </remarks>
        // TODO: need another way to do it, eg an interface, injected!
        public static Func<HttpRequestBase, string> ApplicationUrlProvider { get; set; }

        internal static string GetApplicationUrl(ILogger logger, IGlobalSettings globalSettings, IUmbracoSettingsSection settings, IServerRegistrar serverRegistrar, HttpRequestBase request = null)
        {
            var umbracoApplicationUrl = TryGetApplicationUrl(settings, logger, globalSettings, serverRegistrar);
            if (umbracoApplicationUrl != null)
                return umbracoApplicationUrl;

            umbracoApplicationUrl = ApplicationUrlProvider?.Invoke(request);
            if (string.IsNullOrWhiteSpace(umbracoApplicationUrl) == false)
            {
                umbracoApplicationUrl = umbracoApplicationUrl.TrimEnd(Constants.CharArrays.ForwardSlash);
                logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: {UmbracoAppUrl} (provider)", umbracoApplicationUrl);
                return umbracoApplicationUrl;
            }

            if (request == null) return null;

            umbracoApplicationUrl = GetApplicationUrlFromCurrentRequest(request, globalSettings);
            logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: {UmbracoAppUrl} (UmbracoModule request)", umbracoApplicationUrl);
            return umbracoApplicationUrl;
        }

        internal static string TryGetApplicationUrl(IUmbracoSettingsSection settings, ILogger logger, IGlobalSettings globalSettings, IServerRegistrar serverRegistrar)
        {
            // try umbracoSettings:settings/web.routing/@umbracoApplicationUrl
            // which is assumed to:
            // - end with SystemDirectories.Umbraco
            // - contain a scheme
            // - end or not with a slash, it will be taken care of
            // eg "http://www.mysite.com/umbraco"
            var url = settings.WebRouting.UmbracoApplicationUrl;
            if (url.IsNullOrWhiteSpace() == false)
            {
                var umbracoApplicationUrl = url.TrimEnd(Constants.CharArrays.ForwardSlash);
                logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: {UmbracoAppUrl} (using web.routing/@umbracoApplicationUrl)", umbracoApplicationUrl);
                return umbracoApplicationUrl;
            }

            // try the server registrar
            // which is assumed to return a URL that:
            // - end with SystemDirectories.Umbraco
            // - contain a scheme
            // - end or not with a slash, it will be taken care of
            // eg "http://www.mysite.com/umbraco"
            url = serverRegistrar.GetCurrentServerUmbracoApplicationUrl();
            if (url.IsNullOrWhiteSpace() == false)
            {
                var umbracoApplicationUrl = url.TrimEnd(Constants.CharArrays.ForwardSlash);
                logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: {UmbracoAppUrl} (IServerRegistrar)", umbracoApplicationUrl);
                return umbracoApplicationUrl;
            }

            // else give up...
            return null;
        }

        public static string GetApplicationUrlFromCurrentRequest(HttpRequestBase request, IGlobalSettings globalSettings)
        {
            // if (HTTP and SSL not required) or (HTTPS and SSL required),
            //  use ports from request
            // otherwise,
            //  if non-standard ports used,
            //  user may need to set umbracoApplicationUrl manually per
            //  https://our.umbraco.com/documentation/Using-Umbraco/Config-files/umbracoSettings/#ScheduledTasks
            var port = (request.IsSecureConnection == false && globalSettings.UseHttps == false)
                        || (request.IsSecureConnection && globalSettings.UseHttps)
                ? ":" + request.ServerVariables["SERVER_PORT"]
                : "";

            var ssl = globalSettings.UseHttps ? "s" : ""; // force, whatever the first request
            var url = "http" + ssl + "://" + request.ServerVariables["SERVER_NAME"] + port + IOHelper.ResolveUrl(SystemDirectories.Umbraco);

            return url.TrimEnd(Constants.CharArrays.ForwardSlash);
        }

        /// <summary>
        /// Will get the application URL from configuration, if none is specified will fall back to URL from request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="umbracoSettingsSection"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static Uri GetApplicationUriUncached(
            HttpRequestBase request,
            IUmbracoSettingsSection umbracoSettingsSection,
            IGlobalSettings globalSettings)
        {
            var settingUrl = umbracoSettingsSection.WebRouting.UmbracoApplicationUrl;


            if (string.IsNullOrEmpty(settingUrl))
            {
                if (!Uri.TryCreate(request.Url, VirtualPathUtility.ToAbsolute(globalSettings.Path), out var result))
                {
                    throw new InvalidOperationException(
                        $"Could not create an url from {request.Url} and {globalSettings.Path}");
                }
                return result;
            }
            else
            {
                return new Uri(settingUrl);
            }
        }
    }
}
