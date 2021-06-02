using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the main Umbraco module.
    /// </summary>
    /// <remarks>
    /// <para>Register that one in web.config.</para>
    /// <para>It will inject <see cref="UmbracoInjectedModule"/> which contains most of the actual code.</para>
    /// </remarks>
    public class UmbracoModule : ModuleInjector<UmbracoInjectedModule>
    {
        /// <summary>
        /// Occurs when...
        /// </summary>
        internal static event EventHandler<RoutableAttemptEventArgs> RouteAttempt;

        /// <summary>
        /// Occurs when...
        /// </summary>
        public static event EventHandler<UmbracoRequestEventArgs> EndRequest;

        /// <summary>
        /// Triggers the RouteAttempt event.
        /// </summary>
        internal static void OnRouteAttempt(object sender, RoutableAttemptEventArgs args)
        {
            RouteAttempt?.Invoke(sender, args);
        }

        /// <summary>
        /// Triggers the EndRequest event.
        /// </summary>
        internal static void OnEndRequest(object sender, UmbracoRequestEventArgs args)
        {
            EndRequest?.Invoke(sender, args);
        }

        // returns a value indicating whether redirection took place and the request has
        // been completed - because we don't want to Response.End() here to terminate
        // everything properly.
        internal static bool HandleHttpResponseStatus(HttpContextBase context, PublishedRequest pcr, ILogger logger)
        {
            var end = false;
            var response = context.Response;

            logger.Debug<UmbracoModule, string, string, int>("Response status: Redirect={Redirect}, Is404={Is404}, StatusCode={ResponseStatusCode}",
                pcr.IsRedirect ? (pcr.IsRedirectPermanent ? "permanent" : "redirect") : "none",
                pcr.Is404 ? "true" : "false",
                pcr.ResponseStatusCode);

            if(pcr.Cacheability != default)
                response.Cache.SetCacheability(pcr.Cacheability);

            foreach (var cacheExtension in pcr.CacheExtensions)
                response.Cache.AppendCacheExtension(cacheExtension);

            foreach (var header in pcr.Headers)
                response.AppendHeader(header.Key, header.Value);

            if (pcr.IsRedirect)
            {
                if (pcr.IsRedirectPermanent)
                    response.RedirectPermanent(pcr.RedirectUrl, false); // do not end response
                else
                    response.Redirect(pcr.RedirectUrl, false); // do not end response
                end = true;
            }
            else if (pcr.Is404)
            {
                response.StatusCode = 404;
                response.TrySkipIisCustomErrors = Current.Configs.Settings().WebRouting.TrySkipIisCustomErrors;

                if (response.TrySkipIisCustomErrors == false)
                    logger.Warn<UmbracoModule>("Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over.");
            }

            if (pcr.ResponseStatusCode > 0)
            {
                // set status code -- even for redirects
                response.StatusCode = pcr.ResponseStatusCode;
                response.StatusDescription = pcr.ResponseStatusDescription;
            }
            //if (pcr.IsRedirect)
            //    response.End(); // end response -- kills the thread and does not return!

            if (pcr.IsRedirect == false) return end;

            response.Flush();
            // bypass everything and directly execute EndRequest event -- but returns
            context.ApplicationInstance.CompleteRequest();
            // though some say that .CompleteRequest() does not properly shutdown the response
            // and the request will hang until the whole code has run... would need to test?
            logger.Debug<UmbracoModule>("Response status: redirecting, complete request now.");

            return end;
        }
    }
}
