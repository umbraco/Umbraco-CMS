using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebServices
{
    public abstract class UmbracoHttpHandler : IHttpHandler
    {
        public abstract void ProcessRequest(HttpContext context);
        public abstract bool IsReusable { get; }

        protected UmbracoHttpHandler()
            : this(UmbracoContext.Current)
        {

        }

        protected UmbracoHttpHandler(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            Umbraco = new UmbracoHelper(umbracoContext);
        }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        /// <summary>
        /// Returns an ILogger
        /// </summary>
        public ILogger Logger
        {
            get { return ProfilingLogger.Logger; }
        }

        /// <summary>
        /// Returns a ProfilingLogger
        /// </summary>
        public ProfilingLogger ProfilingLogger
        {
            get { return UmbracoContext.Application.ProfilingLogger; }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco { get; private set; }

        private UrlHelper _url;

        /// <summary>
        /// Returns a UrlHelper
        /// </summary>
        /// <remarks>
        /// This URL helper is created without any route data and an empty request context
        /// </remarks>
        public UrlHelper Url
        {
            get { return _url ?? (_url = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()))); }
        }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        public ServiceContext Services
        {
            get { return ApplicationContext.Services; }
        }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        public DatabaseContext DatabaseContext
        {
            get { return ApplicationContext.DatabaseContext; }
        }

        /// <summary>
        /// Returns a WebSecurity instance
        /// </summary>
        public WebSecurity Security
        {
            get { return UmbracoContext.Security; }
        }
    }
}