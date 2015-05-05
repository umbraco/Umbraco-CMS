using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
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
        protected UmbracoController(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
        }

        protected UmbracoController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (umbracoHelper == null) throw new ArgumentNullException("umbracoHelper");
            UmbracoContext = umbracoContext;
            _umbraco = umbracoHelper;
        }

        protected UmbracoController()
            : this(UmbracoContext.Current)
        {
            
        }

        protected IOwinContext OwinContext
        {
            get { return Request.GetOwinContext(); }
        }

        private UmbracoHelper _umbraco;

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        public MembershipHelper Members
        {
            get { return Umbraco.MembershipHelper; }
        }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public virtual UmbracoHelper Umbraco
        {
            get { return _umbraco ?? (_umbraco = new UmbracoHelper(UmbracoContext)); }
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
        public virtual ProfilingLogger ProfilingLogger
        {
            get { return UmbracoContext.Application.ProfilingLogger; }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public virtual UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public virtual ApplicationContext ApplicationContext
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
        public virtual WebSecurity Security
        {
            get { return UmbracoContext.Security; }
        }
    }
}