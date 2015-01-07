using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A base controller class containing all of the Umbraco objects as properties that a developer requires
    /// </summary>
    public abstract class UmbracoController : Controller
    {
        protected UmbracoController(ILogger logger, UmbracoContext umbracoContext)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            Logger = logger;
            UmbracoContext = umbracoContext;
        }

        protected UmbracoController(UmbracoContext umbracoContext)
            : this(LoggerResolver.Current.Logger, umbracoContext)
        {            
        }

        protected UmbracoController()
            : this(UmbracoContext.Current)
        {
            
        }

        private UmbracoHelper _umbraco;

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco
        {
            get { return _umbraco ?? (_umbraco = new UmbracoHelper(UmbracoContext)); }
        }

        /// <summary>
        /// Returns an ILogger
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
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
        /// Returns the WebSecurity instance
        /// </summary>
        public WebSecurity Security
        {
            get { return UmbracoContext.Security; }
        }
    }
}