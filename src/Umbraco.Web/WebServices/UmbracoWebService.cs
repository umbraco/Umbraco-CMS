using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Services;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// An abstract web service class exposing common umbraco objects
    /// </summary>
    public abstract class UmbracoWebService : WebService
    {
        private UrlHelper _url;

        protected UmbracoWebService()
            : this(Current.UmbracoContext, Current.Services, Current.ApplicationCache)
        { }

        protected UmbracoWebService(UmbracoContext umbracoContext, ServiceContext services, CacheHelper appCache)
        {
            UmbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            Umbraco = new UmbracoHelper(umbracoContext, services, appCache);

            // fixme inject somehow
            Logger = Current.Logger;
            ProfilingLogger = Current.ProfilingLogger;
            Services = Current.Services;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the ProfilingLogger.
        /// </summary>
        public ProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public WebSecurity Security => UmbracoContext.Security;

        /// <summary>
        /// Gets the Url helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(Context.Request.RequestContext));
    }
}