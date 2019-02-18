using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Provides a base class for Umbraco controllers.
    /// </summary>
    public abstract class UmbracoController : Controller
    {
        // for debugging purposes
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        public IGlobalSettings GlobalSettings { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public virtual UmbracoContext UmbracoContext => UmbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// Gets or sets the Umbraco context accessor.
        /// </summary>
        public virtual IUmbracoContextAccessor UmbracoContextAccessor { get; set; }

        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public AppCaches AppCaches { get; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets or sets the profiling logger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; set; }

        protected IOwinContext OwinContext => Request.GetOwinContext();

        /// <summary>
        /// Gets the membership helper.
        /// </summary>
        public MembershipHelper Members => Umbraco.MembershipHelper;

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public virtual WebSecurity Security => UmbracoContext.Security;

        protected UmbracoController()
            : this(
                  Current.Factory.GetInstance<IGlobalSettings>(),
                  Current.Factory.GetInstance<IUmbracoContextAccessor>(),
                  Current.Factory.GetInstance<ServiceContext>(),
                  Current.Factory.GetInstance<AppCaches>(),
                  Current.Factory.GetInstance<IProfilingLogger>(),
                  Current.Factory.GetInstance<UmbracoHelper>()
            )
        {
        }

        protected UmbracoController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper)
        {
            GlobalSettings = globalSettings;
            UmbracoContextAccessor = umbracoContextAccessor;
            Services = services;
            AppCaches = appCaches;
            Logger = profilingLogger;
            ProfilingLogger = profilingLogger;
            Umbraco = umbracoHelper;
        }
    }
}
