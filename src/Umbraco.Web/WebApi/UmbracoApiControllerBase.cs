using System;
using System.Web;
using System.Web.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Security;

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
                Current.Factory.GetRequiredService<GlobalSettings>(),
                Current.Factory.GetRequiredService<IUmbracoContextAccessor>(),
                Current.Factory.GetRequiredService<IBackOfficeSecurityAccessor>(),
                Current.Factory.GetRequiredService<ISqlContext>(),
                Current.Factory.GetRequiredService<ServiceContext>(),
                Current.Factory.GetRequiredService<AppCaches>(),
                Current.Factory.GetRequiredService<IProfilingLogger>(),
                Current.Factory.GetRequiredService<IRuntimeState>(),
                Current.Factory.GetRequiredService<UmbracoMapper>(),
                Current.Factory.GetRequiredService<IPublishedUrlProvider>()
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApiControllerBase"/> class with all its dependencies.
        /// </summary>
        protected UmbracoApiControllerBase(GlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, IBackOfficeSecurityAccessor  backOfficeSecurityAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
        {
            UmbracoContextAccessor = umbracoContextAccessor;
            BackOfficeSecurityAccessor = backOfficeSecurityAccessor;
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

        public IBackOfficeSecurityAccessor BackOfficeSecurityAccessor { get; }


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
        public IBackOfficeSecurity Security => BackOfficeSecurityAccessor.BackOfficeSecurity;


    }
}
