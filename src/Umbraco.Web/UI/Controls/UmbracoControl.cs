using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.UI.Controls
{
    /// <summary>
	/// A control that exposes the helpful Umbraco context objects
	/// </summary>
	public abstract class UmbracoControl : Control
	{
        private UrlHelper _url;

        protected UmbracoControl(UmbracoContext umbracoContext, ServiceContext services, CacheHelper appCache)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            UmbracoContext = umbracoContext;
            Umbraco = new UmbracoHelper(umbracoContext, services, appCache);

            // fixme inject somehow
            Logger = Current.Logger;
            ProfilingLogger = Current.ProfilingLogger;
            DatabaseContext = Current.DatabaseContext;
            Services = Current.Services;
        }

        /// <summary>
        /// Empty constructor, uses Singleton to resolve the UmbracoContext.
        /// </summary>
        protected UmbracoControl()
            : this(Current.UmbracoContext, Current.Services, Current.ApplicationCache)
        { }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

	    /// <summary>
        /// Gets the profiling logger.
        /// </summary>
        public ProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public UmbracoContext UmbracoContext { get; }

	    /// <summary>
	    /// Gets the database context.
	    /// </summary>
	    protected DatabaseContext DatabaseContext { get; }

	    /// <summary>
	    /// Gets the services context.
	    /// </summary>
	    protected ServiceContext Services { get; }

        /// <summary>
        /// Gets a Url helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData())));
	}
}