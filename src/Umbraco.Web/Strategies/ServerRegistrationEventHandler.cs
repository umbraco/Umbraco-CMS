using System;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
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
        private readonly object _locko = new object();
        private DatabaseServerRegistrar _registrar;
        private DateTime _lastUpdated = DateTime.MinValue;

        // bind to events
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _registrar = ServerRegistrarResolver.Current.Registrar as DatabaseServerRegistrar;

            // only for the DatabaseServerRegistrar
            if (_registrar == null) return;

            UmbracoModule.RouteAttempt += UmbracoModuleRouteAttempt;
        }

        // handles route attempts.
        private void UmbracoModuleRouteAttempt(object sender, RoutableAttemptEventArgs e)
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
        private void RegisterServer(UmbracoRequestEventArgs e)
        {
            lock (_locko) // ensure we trigger only once
            {
                var secondsSinceLastUpdate = DateTime.Now.Subtract(_lastUpdated).TotalSeconds;
                if (secondsSinceLastUpdate < _registrar.Options.ThrottleSeconds) return;
                _lastUpdated = DateTime.Now;
            }

            var svc = e.UmbracoContext.Application.Services.ServerRegistrationService;

            // because
            // - ApplicationContext.UmbracoApplicationUrl is initialized by UmbracoModule in BeginRequest
            // - RegisterServer is called on UmbracoModule.RouteAttempt which is triggered in ProcessRequest
            // we are safe, UmbracoApplicationUrl has been initialized
            var serverAddress = e.UmbracoContext.Application.UmbracoApplicationUrl;

            try
            {
                svc.TouchServer(serverAddress, svc.CurrentServerIdentity, _registrar.Options.StaleServerTimeout);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ServerRegistrationEventHandler>("Failed to update server record in database.", ex);
            }
        }
    }
}
