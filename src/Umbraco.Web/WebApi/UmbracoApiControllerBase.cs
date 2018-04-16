using System;
using System.Web;
using System.Web.Http;
using LightInject;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Provides a base class for Umbraco API controllers.
    /// </summary>
    /// <remarks>These controllers are NOT auto-routed.</remarks>
    [FeatureAuthorize]
    public abstract class UmbracoApiControllerBase : ApiController
    {
        private UmbracoHelper _umbracoHelper;

        // for debugging purposes
        internal Guid InstanceId { get; } = Guid.NewGuid();

        // note
        // properties marked as [Inject] below will be property-injected (vs constructor-injected) in
        // order to keep the constuctor as light as possible, so that ppl implementing eg a SurfaceController
        // don't need to implement complex constructors + need to refactor them each time we change ours.
        // this means that these properties have a setter.
        // what can go wrong?

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        [Inject]
        public virtual IGlobalSettings GlobalSettings { get; set; }

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        [Inject]
        public virtual UmbracoContext UmbracoContext { get; set; }

        /// <summary>
        /// Gets or sets the sql context.
        /// </summary>
        [Inject]
        public ISqlContext SqlContext { get; set; }

        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        [Inject]
        public ServiceContext Services { get; set; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        [Inject]
        public CacheHelper ApplicationCache { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        [Inject]
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the profiling logger.
        /// </summary>
        [Inject]
        public ProfilingLogger ProfilingLogger { get; set; }

        /// <summary>
        /// Gets or sets the runtime state.
        /// </summary>
        [Inject]
        internal IRuntimeState RuntimeState { get; set; }

        /// <summary>
        /// Gets the application url.
        /// </summary>
        protected Uri ApplicationUrl => RuntimeState.ApplicationUrl;

        /// <summary>
        /// Gets the membership helper.
        /// </summary>
        public MembershipHelper Members => Umbraco.MembershipHelper;

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco => _umbracoHelper
            ?? (_umbracoHelper = new UmbracoHelper(UmbracoContext, Services, ApplicationCache));

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public WebSecurity Security => UmbracoContext.Security;

        /// <summary>
        /// Tries to get the current HttpContext.
        /// </summary>
        protected Attempt<HttpContextBase> TryGetHttpContext()
        {
            return Request.TryGetHttpContext();
        }

        /// <summary>
        /// Tries to get the current OWIN context.
        /// </summary>
        protected Attempt<IOwinContext> TryGetOwinContext()
        {
            return Request.TryGetOwinContext();
        }
    }
}
