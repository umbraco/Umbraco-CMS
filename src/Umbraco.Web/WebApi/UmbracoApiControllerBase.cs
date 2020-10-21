using System;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi
{


    /// <summary>
    /// Provides a base class for Umbraco API controllers.
    /// </summary>
    /// <remarks>These controllers are NOT auto-routed.</remarks>
    [FeatureAuthorize]
    public abstract class UmbracoApiControllerBase : ApiController, IUmbracoFeature
    {

        // note: all Umbraco controllers have two constructors: one with all dependencies, which should be used,
        // and one with auto dependencies, ie no dependencies - and then dependencies are automatically obtained
        // here from the Current service locator - this is obviously evil, but it allows us to add new dependencies
        // without breaking compatibility.

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApiControllerBase"/> class with auto dependencies.
        /// </summary>
        /// <remarks>Dependencies are obtained from the <see cref="Current"/> service locator.</remarks>
        protected UmbracoApiControllerBase()
            : this(
                Current.Factory.GetInstance<GlobalSettings>(),
                Current.Factory.GetInstance<IUmbracoContextAccessor>(),
                Current.Factory.GetInstance<ISqlContext>(),
                Current.Factory.GetInstance<ServiceContext>(),
                Current.Factory.GetInstance<AppCaches>(),
                Current.Factory.GetInstance<IProfilingLogger>(),
                Current.Factory.GetInstance<IRuntimeState>(),
                Current.Factory.GetInstance<UmbracoMapper>(),
                Current.Factory.GetInstance<IPublishedUrlProvider>()
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApiControllerBase"/> class with all its dependencies.
        /// </summary>
        protected UmbracoApiControllerBase(GlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
        {
            UmbracoContextAccessor = umbracoContextAccessor;
            SqlContext = sqlContext;
            Services = services;
            AppCaches = appCaches;
            Logger = logger;
            RuntimeState = runtimeState;
            Mapper = umbracoMapper;
            PublishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Gets a unique instance identifier.
        /// </summary>
        /// <remarks>For debugging purposes.</remarks>
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public virtual IUmbracoContext UmbracoContext => UmbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// Gets the Umbraco context accessor.
        /// </summary>
        public virtual IUmbracoContextAccessor UmbracoContextAccessor { get; }


        /// <summary>
        /// Gets the sql context.
        /// </summary>
        public ISqlContext SqlContext { get; }

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets the application cache.
        /// </summary>
        public AppCaches AppCaches { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public IProfilingLogger Logger { get; }

        /// <summary>
        /// Gets the runtime state.
        /// </summary>
        internal IRuntimeState RuntimeState { get; }

        /// <summary>
        /// Gets the mapper.
        /// </summary>
        public UmbracoMapper Mapper { get; }

        protected IPublishedUrlProvider PublishedUrlProvider { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public IBackOfficeSecurity Security => UmbracoContext.Security;

        /// <summary>
        /// Tries to get the current HttpContext.
        /// </summary>
        protected Attempt<HttpContextBase> TryGetHttpContext()
            => Request.TryGetHttpContext();

        /// <summary>
        /// Tries to get the current OWIN context.
        /// </summary>
        protected Attempt<IOwinContext> TryGetOwinContext()
            => Request.TryGetOwinContext();
    }
}
