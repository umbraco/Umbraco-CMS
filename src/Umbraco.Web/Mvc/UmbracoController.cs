using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
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
        public IOptions<GlobalSettings> GlobalSettings { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public virtual IUmbracoContext UmbracoContext => UmbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// Gets or sets the Umbraco context accessor.
        /// </summary>
        public IUmbracoContextAccessor UmbracoContextAccessor { get; set; }

        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public AppCaches AppCaches { get; }


        /// <summary>
        /// Gets or sets the profiling logger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; set; }

        /// <summary>
        /// Gets the LoggerFactory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        protected IOwinContext OwinContext => Request.GetOwinContext();

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public virtual IBackofficeSecurity Security => UmbracoContext.Security;

        protected UmbracoController()
            : this(
                  Current.Factory.GetRequiredService<IOptions<GlobalSettings>>(),
                  Current.Factory.GetRequiredService<IUmbracoContextAccessor>(),
                  Current.Factory.GetRequiredService<ServiceContext>(),
                  Current.Factory.GetRequiredService<AppCaches>(),
                  Current.Factory.GetRequiredService<IProfilingLogger>(),
                  Current.Factory.GetRequiredService<LoggerFactory>()
            )
        {
        }

        protected UmbracoController(IOptions<GlobalSettings> globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, ILoggerFactory loggerFactory)
        {
            GlobalSettings = globalSettings;
            UmbracoContextAccessor = umbracoContextAccessor;
            Services = services;
            AppCaches = appCaches;
            ProfilingLogger = profilingLogger;
            LoggerFactory = loggerFactory;
        }
    }
}
