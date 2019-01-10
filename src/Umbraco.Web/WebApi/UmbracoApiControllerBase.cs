﻿using System;
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
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private UmbracoHelper _umbracoHelper;

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
                Current.Factory.GetInstance<IGlobalSettings>(),
                Current.Factory.GetInstance<IUmbracoContextAccessor>(),
                Current.Factory.GetInstance<ISqlContext>(),
                Current.Factory.GetInstance<ServiceContext>(),
                Current.Factory.GetInstance<CacheHelper>(),
                Current.Factory.GetInstance<IProfilingLogger>(),
                Current.Factory.GetInstance<IRuntimeState>()
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApiControllerBase"/> class with all its dependencies.
        /// </summary>
        protected UmbracoApiControllerBase(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, IProfilingLogger logger, IRuntimeState runtimeState)
        {
            GlobalSettings = globalSettings;
            _umbracoContextAccessor = umbracoContextAccessor;
            SqlContext = sqlContext;
            Services = services;
            ApplicationCache = applicationCache;
            Logger = logger;
            RuntimeState = runtimeState;
        }

        /// <summary>
        /// Gets a unique instance identifier.
        /// </summary>
        /// <remarks>For debugging purposes.</remarks>
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public virtual IGlobalSettings GlobalSettings { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public virtual UmbracoContext UmbracoContext => _umbracoContextAccessor.UmbracoContext;

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
        public CacheHelper ApplicationCache { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public IProfilingLogger Logger { get; }

        /// <summary>
        /// Gets the runtime state.
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
            ?? (_umbracoHelper = new UmbracoHelper(UmbracoContext, Services));

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public WebSecurity Security => UmbracoContext.Security;

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
