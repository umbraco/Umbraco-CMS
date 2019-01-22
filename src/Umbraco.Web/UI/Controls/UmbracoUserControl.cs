using System;
using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using Umbraco.Web.UI.Pages;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.UI.Controls
{
    /// <summary>
    /// A base class for all Presentation UserControls to inherit from
    /// </summary>
    public abstract class UmbracoUserControl : UserControl
    {
        private ClientTools _clientTools;
        private UrlHelper _url;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="services"></param>
        protected UmbracoUserControl(UmbracoContext umbracoContext, ServiceContext services)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            UmbracoContext = umbracoContext;
            Umbraco = new UmbracoHelper(umbracoContext, services);
            Members = Current.Factory.GetInstance<MembershipHelper>();

            // todo inject somehow
            Logger = Current.Logger;
            ProfilingLogger = Current.ProfilingLogger;
            Services = Current.Services;
        }

        /// <summary>
        /// Empty constructor, uses Singleton to resolve the UmbracoContext
        /// </summary>
        protected UmbracoUserControl()
            : this(Current.UmbracoContext, Current.Services)
        { }

        // for debugging purposes
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Gets the membership helper;
        /// </summary>
        public MembershipHelper Members { get; }

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public WebSecurity Security => UmbracoContext.Security;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the ProfilingLogger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets an instance of ClientTools for access to the pages client API.
        /// </summary>
        public ClientTools ClientTools
        {
            get
            {
                var page = Page as BasePage;
                return _clientTools ?? (_clientTools = page != null ? page.ClientTools : new ClientTools(Page));
            }
        }

        /// <summary>
        /// Gets a Url helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(Context.Request.RequestContext));
    }
}
