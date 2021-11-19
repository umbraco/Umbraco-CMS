using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public abstract class UmbracoHttpHandler : IHttpHandler
    {
        private UrlHelper _url;

        protected UmbracoHttpHandler()
            : this(Current.UmbracoContextAccessor, Current.UmbracoHelper, Current.Services, Current.ProfilingLogger)
        { }

        protected UmbracoHttpHandler(IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper umbracoHelper, ServiceContext service, IProfilingLogger plogger)
        {
            UmbracoContextAccessor = umbracoContextAccessor;
            Logger = plogger;
            ProfilingLogger = plogger;
            Services = service;
            Umbraco = umbracoHelper;
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
        public IProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets the Umbraco context accessor.
        /// </summary>
        public IUmbracoContextAccessor UmbracoContextAccessor { get; }

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
        public WebSecurity Security => UmbracoContextAccessor.UmbracoContext.Security;

        /// <summary>
        /// Gets the URL helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(HttpContext.Current.Request.RequestContext));
    }
}
