using System;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
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
        // fixme remove that comment?!

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        public virtual IGlobalSettings GlobalSettings { get; }

        /// <summary>
        /// Gets or sets the Umbraco context.
        /// </summary>
        public virtual UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets or sets the sql context.
        /// </summary>
        public ISqlContext SqlContext { get; }

        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public CacheHelper ApplicationCache { get; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets or sets the profiling logger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets or sets the runtime state.
        /// </summary>
        internal IRuntimeState RuntimeState { get; }

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

        protected UmbracoApiControllerBase()
            : this(
                  Current.Container.GetInstance<IGlobalSettings>(),
                  Current.Container.GetInstance<UmbracoContext>(),
                  Current.Container.GetInstance<ISqlContext>(),
                  Current.Container.GetInstance<ServiceContext>(),
                  Current.Container.GetInstance<CacheHelper>(),
                  Current.Container.GetInstance<ILogger>(),
                  Current.Container.GetInstance<IProfilingLogger>(),
                  Current.Container.GetInstance<IRuntimeState>()
            )
        {
        }

        // fixme - Inject fewer things? (Aggregate more)
        protected UmbracoApiControllerBase(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, ILogger logger, IProfilingLogger profilingLogger, IRuntimeState runtimeState)
        {
            GlobalSettings = globalSettings;
            UmbracoContext = umbracoContext;
            SqlContext = sqlContext;
            Services = services;
            ApplicationCache = applicationCache;
            Logger = logger;
            ProfilingLogger = profilingLogger;
            RuntimeState = runtimeState;
        }

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
