using System;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// Provides a base class for Umbraco API controllers.
    /// </summary>
    /// <remarks>These controllers are NOT auto-routed.</remarks>
    [FeatureAuthorize]
    public abstract class UmbracoApiControllerBase : Controller, IUmbracoFeature
    {

        //
        // /// <summary>
        // /// Initializes a new instance of the <see cref="UmbracoApiControllerBase"/> class with all its dependencies.
        // /// </summary>
        // protected UmbracoApiControllerBase(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
        // {
        //     UmbracoContextAccessor = umbracoContextAccessor;
        //     GlobalSettings = globalSettings;
        //     SqlContext = sqlContext;
        //     Services = services;
        //     AppCaches = appCaches;
        //     Logger = logger;
        //     RuntimeState = runtimeState;
        //     Mapper = umbracoMapper;
        //     PublishedUrlProvider = publishedUrlProvider;
        // }
        //
        // /// <summary>
        // /// Gets a unique instance identifier.
        // /// </summary>
        // /// <remarks>For debugging purposes.</remarks>
        // internal Guid InstanceId { get; } = Guid.NewGuid();
        //
        // /// <summary>
        // /// Gets the Umbraco context.
        // /// </summary>
        // public virtual IGlobalSettings GlobalSettings { get; }
        //
        // /// <summary>
        // /// Gets the Umbraco context.
        // /// </summary>
        // public virtual IUmbracoContext UmbracoContext => UmbracoContextAccessor.UmbracoContext;
        //
        // /// <summary>
        // /// Gets the Umbraco context accessor.
        // /// </summary>
        // public virtual IUmbracoContextAccessor UmbracoContextAccessor { get; }
        //
        //
        // /// <summary>
        // /// Gets the sql context.
        // /// </summary>
        // public ISqlContext SqlContext { get; }
        //
        // /// <summary>
        // /// Gets the services context.
        // /// </summary>
        // public ServiceContext Services { get; }
        //
        // /// <summary>
        // /// Gets the application cache.
        // /// </summary>
        // public AppCaches AppCaches { get; }
        //
        // /// <summary>
        // /// Gets the logger.
        // /// </summary>
        // public IProfilingLogger Logger { get; }
        //
        // /// <summary>
        // /// Gets the runtime state.
        // /// </summary>
        // internal IRuntimeState RuntimeState { get; }
        //
        // /// <summary>
        // /// Gets the application url.
        // /// </summary>
        // protected Uri ApplicationUrl => RuntimeState.ApplicationUrl;
        //
        // /// <summary>
        // /// Gets the mapper.
        // /// </summary>
        // public UmbracoMapper Mapper { get; }
        //
        // protected IPublishedUrlProvider PublishedUrlProvider { get; }
        //
        // /// <summary>
        // /// Gets the web security helper.
        // /// </summary>
        // public IWebSecurity Security => UmbracoContext.Security;
    }
}
