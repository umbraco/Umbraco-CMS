using System.Web.Mvc;
using System.Web.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web
{
    /// <summary>
    /// An abstract web service class exposing common umbraco objects
    /// </summary>
    public abstract class UmbracoWebService : WebService
    {
        private UrlHelper _url;

        protected UmbracoWebService(ILogger logger, IProfilingLogger profilingLogger, IUmbracoContextAccessor umbracoContextAccessor, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, ServiceContext services, GlobalSettings globalSettings)
        {
            Logger = logger;
            ProfilingLogger = profilingLogger;
            UmbracoContextAccessor = umbracoContextAccessor;
            BackOfficeSecurityAccessor = backOfficeSecurityAccessor;
            Services = services;
            GlobalSettings = globalSettings;
        }

        protected UmbracoWebService()
        : this(Current.Logger, Current.ProfilingLogger, Current.UmbracoContextAccessor, Current.BackOfficeSecurityAccessor, Current.Services,new GlobalSettings())
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

        public IBackOfficeSecurityAccessor BackOfficeSecurityAccessor { get; }

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets the global settings.
        /// </summary>
        public GlobalSettings GlobalSettings { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public IBackOfficeSecurity Security => BackOfficeSecurityAccessor.BackOfficeSecurity;

        /// <summary>
        /// Gets the URL helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(Context.Request.RequestContext));
    }
}
