using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// This will ensure that the server is automatically registered in the database as an active node 
    /// on application startup and whenever a back office request occurs.
    /// </summary>
    /// <remarks>
    /// We do this on app startup to ensure that the server is in the database but we also do it for the first 'x' times
    /// a back office request is made so that we can tell if they are using https protocol which would update to that address
    /// in the database. The first front-end request probably wouldn't be an https request.
    /// 
    /// For back office requests (so that we don't constantly make db calls), we'll only update the database when we detect at least
    /// a timespan of 1 minute between requests.
    /// </remarks>
    public sealed class ServerRegistrationEventHandler : ApplicationEventHandler
    {
        private static bool _initUpdated = false;
        private static DateTime _lastUpdated = DateTime.MinValue;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Update the database with this entry and bind to request events
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //bind to event
            UmbracoModule.RouteAttempt += UmbracoModuleRouteAttempt;
        }


        static void UmbracoModuleRouteAttempt(object sender, Routing.RoutableAttemptEventArgs e)
        {
            if (e.HttpContext.Request == null || e.HttpContext.Request.Url == null) return;

            if (e.Outcome == EnsureRoutableOutcome.IsRoutable)
            {
                using (var lck = new UpgradeableReadLock(Locker))
                {
                    //we only want to do the initial update once
                    if (!_initUpdated)
                    {
                        lck.UpgradeToWriteLock();
                        _initUpdated = true;
                        UpdateServerEntry(e.HttpContext, e.UmbracoContext.Application);
                        return;
                    }
                }
            }

            //if it is not a document request, we'll check if it is a back end request
            if (e.Outcome == EnsureRoutableOutcome.NotDocumentRequest)
            {
                var authority = e.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                var afterAuthority = e.HttpContext.Request.Url.GetLeftPart(UriPartial.Query)
                                      .TrimStart(authority)
                                      .TrimStart("/");

                //check if this is in the umbraco back office
                if (afterAuthority.InvariantStartsWith(GlobalSettings.Path.TrimStart("/")))
                {
                    //yup it's a back office request!
                    using (var lck = new UpgradeableReadLock(Locker))
                    {
                        //we don't want to update if it's not been at least a minute since last time
                        var isItAMinute = DateTime.Now.Subtract(_lastUpdated).TotalSeconds >= 60;
                        if (isItAMinute)
                        {
                            lck.UpgradeToWriteLock();
                            _initUpdated = true;
                            _lastUpdated = DateTime.Now;
                            UpdateServerEntry(e.HttpContext, e.UmbracoContext.Application);
                        }
                    }
                }
            }
        }


        private static void UpdateServerEntry(HttpContextBase httpContext, ApplicationContext applicationContext)
        {
            try
            {
                var address = httpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                applicationContext.Services.ServerRegistrationService.EnsureActive(address);
            }
            catch (Exception e)
            {
                LogHelper.Error<ServerRegistrationEventHandler>("Failed to update server record in database.", e);
            }
        }
    }
}
