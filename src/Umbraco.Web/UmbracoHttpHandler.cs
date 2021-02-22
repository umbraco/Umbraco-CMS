using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web
{
    public abstract class UmbracoHttpHandler : IHttpHandler
    {
        private UrlHelper _url;

        protected UmbracoHttpHandler()
            : this(Current.UmbracoContextAccessor, Current.BackOfficeSecurityAccessor, Current.Services, Current.Logger, Current.ProfilingLogger)
        { }

        protected UmbracoHttpHandler(IUmbracoContextAccessor umbracoContextAccessor, IBackOfficeSecurityAccessor backOfficeSecurityAccessor,ServiceContext service, ILogger logger, IProfilingLogger profilingLogger )
        {
            UmbracoContextAccessor = umbracoContextAccessor;
            BackOfficeSecurityAccessor = backOfficeSecurityAccessor;
            Logger = logger;
            ProfilingLogger = profilingLogger ;
            Services = service;
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

        public IBackOfficeSecurityAccessor BackOfficeSecurityAccessor { get; }

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public IBackOfficeSecurity Security => BackOfficeSecurityAccessor.BackOfficeSecurity;

        /// <summary>
        /// Gets the URL helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(HttpContext.Current.Request.RequestContext));
    }
}
