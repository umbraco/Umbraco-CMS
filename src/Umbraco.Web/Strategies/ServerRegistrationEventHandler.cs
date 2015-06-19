using System;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Ensures that servers are automatically registered in the database, when using the database server registrar.
    /// </summary>
    /// <remarks>
    /// <para>At the moment servers are automatically registered upon first request and then on every
    /// request but not more than once per (configurable) period. This really is "for information & debug" purposes so
    /// we can look at the table and see what servers are registered - but the info is not used anywhere.</para>
    /// <para>Should we actually want to use this, we would need a better and more deterministic way of figuring
    /// out the "server address" ie the address to which server-to-server requests should be sent - because it
    /// probably is not the "current request address" - especially in multi-domains configurations.</para>
    /// </remarks>
    public sealed class ServerRegistrationEventHandler : ApplicationEventHandler
    {
        private static DateTime _lastUpdated = DateTime.MinValue;

        // bind to events
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // only for the DatabaseServerRegistrar
            if (ServerRegistrarResolver.Current.Registrar is DatabaseServerRegistrar)
                UmbracoModule.RouteAttempt += UmbracoModuleRouteAttempt;
        }

        // handles route attempts.
        private static void UmbracoModuleRouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            if (e.HttpContext.Request == null || e.HttpContext.Request.Url == null) return;

            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                    // front-end request
                    RegisterServer(e);
                    break;
                case EnsureRoutableOutcome.NotDocumentRequest:
                    // anything else (back-end request, service...)
                    //so it's not a document request, we'll check if it's a back office request
                    if (e.HttpContext.Request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath))
                        RegisterServer(e);
                    break;
                /*
                case EnsureRoutableOutcome.NotReady:
                case EnsureRoutableOutcome.NotConfigured:
                case EnsureRoutableOutcome.NoContent:
                default:
                    // otherwise, do nothing
                    break;
                */
            }
        }

        // register current server (throttled).
        private static void RegisterServer(UmbracoRequestEventArgs e)
        {
            var reg = (DatabaseServerRegistrar) ServerRegistrarResolver.Current.Registrar;
            var options = reg.Options;
            var secondsSinceLastUpdate = DateTime.Now.Subtract(_lastUpdated).TotalSeconds;
            if (secondsSinceLastUpdate < options.ThrottleSeconds) return;

            _lastUpdated = DateTime.Now;

            var url = e.HttpContext.Request.Url;
            var svc = e.UmbracoContext.Application.Services.ServerRegistrationService;

            try
            {
                if (url == null)
                    throw new Exception("Request.Url is null.");

                var serverAddress = url.GetLeftPart(UriPartial.Authority);
                var serverIdentity = JsonConvert.SerializeObject(new
                {
                    machineName = NetworkHelper.MachineName, 
                    appDomainAppId = HttpRuntime.AppDomainAppId
                });

                svc.TouchServer(serverAddress, serverIdentity, options.StaleServerTimeout);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ServerRegistrationEventHandler>("Failed to update server record in database.", ex);
            }
        }
    }
}
