using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Umbraco.Web
{
    /// <summary>
    /// An abstract web service class exposing common umbraco objects
    /// </summary>
    public abstract class UmbracoWebService : WebService
    {
        private UrlHelper _url;

        protected UmbracoWebService(ILogger logger, IProfilingLogger profilingLogger, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, IGlobalSettings globalSettings)
        {
            Logger = logger;
            ProfilingLogger = profilingLogger;
            UmbracoContextAccessor = umbracoContextAccessor;
            Services = services;
            GlobalSettings = globalSettings;
        }

        protected UmbracoWebService()
        : this(Current.Logger, Current.ProfilingLogger, Current.UmbracoContextAccessor,  Current.Services, Current.Configs.Global())
        {
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the ProfilingLogger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public IUmbracoContext UmbracoContext => UmbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// Gets the Umbraco context accessor.
        /// </summary>
        public IUmbracoContextAccessor UmbracoContextAccessor { get; }

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets the global settings.
        /// </summary>
        public IGlobalSettings GlobalSettings { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public IWebSecurity Security => UmbracoContext.Security;

        /// <summary>
        /// Gets the Url helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(Context.Request.RequestContext));
    }
}
