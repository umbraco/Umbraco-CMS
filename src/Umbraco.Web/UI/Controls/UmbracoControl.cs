using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace Umbraco.Web.UI.Controls
{
    /// <summary>
	/// A control that exposes the helpful Umbraco context objects
	/// </summary>
	public abstract class UmbracoControl : Control
	{

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected UmbracoControl(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            Umbraco = new UmbracoHelper(umbracoContext);
        }

        /// <summary>
        /// Empty constructor, uses Singleton to resolve the UmbracoContext
        /// </summary>
        protected UmbracoControl()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco { get; private set; }

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
        public ProfilingLogger ProfilingLogger
        {
            get { return UmbracoContext.Application.ProfilingLogger; }
        }

        public UmbracoContext UmbracoContext { get; private set; }

		protected ApplicationContext ApplicationContext
		{
			get { return UmbracoContext.Application; }
		}
		protected DatabaseContext DatabaseContext
		{
			get { return ApplicationContext.DatabaseContext; }
		}
		protected ServiceContext Services
		{
			get { return ApplicationContext.Services; }
		}

        private UrlHelper _url;
        /// <summary>
        /// Returns a UrlHelper
        /// </summary>
        /// <remarks>
        /// This URL helper is created without any route data and an empty request context
        /// </remarks>
        public UrlHelper Url
        {
            get { return _url ?? (_url = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()))); }
        }

		/// <summary>
		/// Returns the legacy SqlHelper
		/// </summary>
		protected ISqlHelper SqlHelper
		{
			get { return Application.SqlHelper; }
		}
	}
}