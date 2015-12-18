using System;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A helper used to determine the current server umbraco application url.
    /// </summary>
    public static class ApplicationUrlHelper
    {
        // because we cannot logger.Info<ApplicationUrlHelper> because type is static
        private static readonly Type TypeOfApplicationUrlHelper = typeof(ApplicationUrlHelper);

        private static Func<HttpRequestBase, string> _applicationUrlProvider;

        /// <summary>
        /// Gets or sets a custom provider for the umbraco application url.
        /// </summary>
        /// <remarks>
        /// <para>Receives the current request as a parameter, and it may be null. Must return a properly
        /// formatted url with scheme and umbraco dir and no trailing slash eg "http://www.mysite.com/umbraco",
        /// or <c>null</c>. To be used in auto-load-balancing scenarios where the application url is not
        /// in config files but is determined programmatically.</para>
        /// <para>Must be assigned before resolution is frozen.</para>
        /// </remarks>
        public static Func<HttpRequestBase, string> ApplicationUrlProvider 
        {
            get
            {
                return _applicationUrlProvider;
            }
            set
            {
                using (Resolution.Configuration)
                {
                    _applicationUrlProvider = value;
                }
            } 
        } 

        // request: will be null if called from ApplicationContext
        // settings: for unit tests only
        internal static void EnsureApplicationUrl(ApplicationContext appContext, HttpRequestBase request = null, IUmbracoSettingsSection settings = null)
        {
            // if initialized, return
            if (appContext._umbracoApplicationUrl != null) return;

            var logger = appContext.ProfilingLogger.Logger;

            // try settings and IServerRegistrar
            if (TrySetApplicationUrl(appContext, settings ?? UmbracoConfig.For.UmbracoSettings()))
                return;

            // try custom provider
            if (_applicationUrlProvider != null)
            {
                var url = _applicationUrlProvider(request);
                if (url.IsNullOrWhiteSpace() == false)
                {
                    appContext._umbracoApplicationUrl = url.TrimEnd('/');
                    logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (provider)");
                    return;
                }
            }

            // last chance,
            // use the current request as application url
            if (request == null) return;
            SetApplicationUrlFromCurrentRequest(appContext, request);
        }

        // internal for tests
        internal static bool TrySetApplicationUrl(ApplicationContext appContext, IUmbracoSettingsSection settings)
        {
            var logger = appContext.ProfilingLogger.Logger;

            // try umbracoSettings:settings/web.routing/@umbracoApplicationUrl
            // which is assumed to:
            // - end with SystemDirectories.Umbraco
            // - contain a scheme
            // - end or not with a slash, it will be taken care of
            // eg "http://www.mysite.com/umbraco"
            var url = settings.WebRouting.UmbracoApplicationUrl;
            if (url.IsNullOrWhiteSpace() == false)
            {
                appContext._umbracoApplicationUrl = url.TrimEnd('/');
                logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (using web.routing/@umbracoApplicationUrl)");
                return true;
            }

            // try umbracoSettings:settings/scheduledTasks/@baseUrl
            // which is assumed to:
            // - end with SystemDirectories.Umbraco
            // - NOT contain any scheme (because, legacy)
            // - end or not with a slash, it will be taken care of
            // eg "mysite.com/umbraco"
            url = settings.ScheduledTasks.BaseUrl;
            if (url.IsNullOrWhiteSpace() == false)
            {
                var ssl = GlobalSettings.UseSSL ? "s" : "";
                url = "http" + ssl + "://" + url;
                appContext._umbracoApplicationUrl = url.TrimEnd('/');
                logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (using scheduledTasks/@baseUrl)");
                return true;
            }

            // try the server registrar
            // which is assumed to return a url that:
            // - end with SystemDirectories.Umbraco
            // - contain a scheme
            // - end or not with a slash, it will be taken care of
            // eg "http://www.mysite.com/umbraco"
            var registrar = ServerRegistrarResolver.Current.Registrar as IServerRegistrar2;
            url = registrar == null ? null : registrar.GetCurrentServerUmbracoApplicationUrl();
            if (url.IsNullOrWhiteSpace() == false)
            {
                appContext._umbracoApplicationUrl = url.TrimEnd('/');
                logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (IServerRegistrar)");
                return true;
            }

            // else give up...
            return false;
        }

        private static void SetApplicationUrlFromCurrentRequest(ApplicationContext appContext, HttpRequestBase request)
        {
            var logger = appContext.ProfilingLogger.Logger;

            // if (HTTP and SSL not required) or (HTTPS and SSL required),
            //  use ports from request
            // otherwise,
            //  if non-standard ports used,
            //  user may need to set umbracoApplicationUrl manually per 
            //  http://our.umbraco.org/documentation/Using-Umbraco/Config-files/umbracoSettings/#ScheduledTasks
            var port = (request.IsSecureConnection == false && GlobalSettings.UseSSL == false)
                        || (request.IsSecureConnection && GlobalSettings.UseSSL)
                ? ":" + request.ServerVariables["SERVER_PORT"]
                : "";

            var ssl = GlobalSettings.UseSSL ? "s" : ""; // force, whatever the first request
            var url = "http" + ssl + "://" + request.ServerVariables["SERVER_NAME"] + port + IOHelper.ResolveUrl(SystemDirectories.Umbraco);

            appContext._umbracoApplicationUrl = url.TrimEnd('/');
            logger.Info(TypeOfApplicationUrlHelper, "ApplicationUrl: " + appContext.UmbracoApplicationUrl + " (UmbracoModule request)");
        }
    }
}