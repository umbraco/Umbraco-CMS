using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using umbraco.DataLayer;

namespace Umbraco.Web.UI.Controls
{
    /// <summary>
    /// A base class for all Presentation UserControls to inherit from
    /// </summary>
    public abstract class UmbracoUserControl : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected UmbracoUserControl(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
            Umbraco = new UmbracoHelper(umbracoContext);
            _membershipHelper = new MembershipHelper(umbracoContext);
        }

        /// <summary>
        /// Empty constructor, uses Singleton to resolve the UmbracoContext
        /// </summary>
        protected UmbracoUserControl()
            : this(UmbracoContext.Current)
        {
        }

        private readonly MembershipHelper _membershipHelper;

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco { get; private set; }

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        public MembershipHelper Members
        {
            get { return _membershipHelper; }
        }

        /// <summary>
        /// Returns the current WebSecurity instance
        /// </summary>
        public WebSecurity Security
        {
            get { return UmbracoContext.Security; }
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
        public ProfilingLogger ProfilingLogger
        {
            get { return UmbracoContext.Application.ProfilingLogger; }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
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
            get { return global::umbraco.BusinessLogic.Application.SqlHelper; }
        }
    }
}