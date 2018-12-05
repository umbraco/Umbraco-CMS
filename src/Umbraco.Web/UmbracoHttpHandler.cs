using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public abstract class UmbracoHttpHandler : IHttpHandler
    {
        private UrlHelper _url;

        protected UmbracoHttpHandler()
            : this(Current.UmbracoContext, Current.Services)
        { }

        protected UmbracoHttpHandler(UmbracoContext umbracoContext, ServiceContext services)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            UmbracoContext = umbracoContext;
            Umbraco = new UmbracoHelper(umbracoContext, services);

            // fixme inject somehow
            Logger = Current.Logger;
            ProfilingLogger = Current.ProfilingLogger;
            Services = Current.Services;
        }

        public abstract void ProcessRequest(HttpContext context);

        public abstract bool IsReusable { get; }

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
        public UrlHelper Url => _url ?? (_url = new UrlHelper(HttpContext.Current.Request.RequestContext));
    }
}
