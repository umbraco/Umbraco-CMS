using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// The base class for API controllers that expose Umbraco services - THESE ARE NOT AUTO ROUTED
    /// </summary>
    [FeatureAuthorize]
    public abstract class UmbracoApiControllerBase : ApiController
    {
        protected UmbracoApiControllerBase()
            : this(UmbracoContext.Current)
        {

        }

        protected UmbracoApiControllerBase(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
            SetRequestCulture();
        }

        protected UmbracoApiControllerBase(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (umbracoHelper == null) throw new ArgumentNullException("umbracoHelper");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
            _umbraco = umbracoHelper;
            SetRequestCulture();
        }

        /// <summary>
        /// Sets the correct thread culture for the current domain if the request is not a back office request
        /// </summary>
        /// <returns>true if the culture was set</returns>
        private void SetRequestCulture()
        {
            var requestUrl = UmbracoContext.OriginalRequestUrl;
            if (requestUrl == null || requestUrl.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath))
            {
                return;
            }

            var domainAndUri = Routing.DomainHelper.DomainForUri(Services.DomainService.GetAll(false), requestUrl);
            if (domainAndUri == null || domainAndUri.UmbracoDomain.LanguageIsoCode.IsNullOrWhiteSpace())
            {
                return;
            }

            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(domainAndUri.UmbracoDomain.LanguageIsoCode);
        }

        private UmbracoHelper _umbraco;

        /// <summary>
        /// Tries to retrieve the current HttpContext if one exists.
        /// </summary>
        /// <returns></returns>
        protected Attempt<HttpContextBase> TryGetHttpContext()
        {
            return Request.TryGetHttpContext();
        }

        /// <summary>
        /// Tries to retrieve the current HttpContext if one exists.
        /// </summary>
        /// <returns></returns>
        protected Attempt<IOwinContext> TryGetOwinContext()
        {
            return Request.TryGetOwinContext();
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
        /// Returns an UmbracoHelper object
        /// </summary>
        public virtual UmbracoHelper Umbraco
        {
            get { return _umbraco ?? (_umbraco = new UmbracoHelper(UmbracoContext)); }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public virtual UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns the WebSecurity instance
        /// </summary>
        public WebSecurity Security
        {
            get { return UmbracoContext.Security; }
        }

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        public MembershipHelper Members
        {
            get { return Umbraco.MembershipHelper; }
        }

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }
    }
}
