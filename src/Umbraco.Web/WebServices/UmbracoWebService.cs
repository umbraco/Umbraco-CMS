using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// An abstract web service class exposing common umbraco objects
    /// </summary>
    public abstract class UmbracoWebService : WebService
    {
        protected UmbracoWebService()
            : this(UmbracoContext.Current)
        {

        }

        protected UmbracoWebService(UmbracoContext umbracoContext)
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
            get { return _url ?? (_url = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()))); }
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