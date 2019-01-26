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
        private UmbracoHelper _umbracoHelper;

        // for debugging purposes
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        public virtual IGlobalSettings GlobalSettings { get; set; }

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        public virtual UmbracoContext UmbracoContext { get; set; }

        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        public ServiceContext Services { get; set; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public AppCaches AppCaches { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Logger { get; set; }

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
        public UmbracoHelper Umbraco => _umbracoHelper
            ?? (_umbracoHelper = new UmbracoHelper(UmbracoContext, Services));

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public virtual WebSecurity Security => UmbracoContext.Security;

        protected UmbracoController()
            : this(
                  Current.Factory.GetInstance<IGlobalSettings>(),
                  Current.Factory.GetInstance<UmbracoContext>(),
                  Current.Factory.GetInstance<ServiceContext>(),
                  Current.Factory.GetInstance<AppCaches>(),
                  Current.Factory.GetInstance<ILogger>(),
                  Current.Factory.GetInstance<IProfilingLogger>()
            )
        {
        }

        protected UmbracoController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger)
        {
            GlobalSettings = globalSettings;
            UmbracoContext = umbracoContext;
            Services = services;
            AppCaches = appCaches;
            Logger = logger;
            ProfilingLogger = profilingLogger;
        }
    }
}
