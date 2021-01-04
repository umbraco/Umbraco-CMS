using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    // TODO: This has been ported to netcore, just needs testing
    public abstract class UmbracoViewPage<TModel> : WebViewPage<TModel>
    {
        private readonly GlobalSettings _globalSettings;
        private readonly ContentSettings _contentSettings;

        private IUmbracoContext _umbracoContext;
        private UmbracoHelper _helper;

        /// <summary>
        /// Gets or sets the database context.
        /// </summary>
        public ServiceContext Services { get; set; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public AppCaches AppCaches { get; set; }

        // TODO: previously, Services and ApplicationCache would derive from UmbracoContext.Application, which
        // was an ApplicationContext - so that everything derived from UmbracoContext.
        // UmbracoContext is fetched from the data tokens, thus allowing the view to be rendered with a
        // custom context and NOT the Current.UmbracoContext - eg outside the normal Umbraco routing
        // process.
        // leaving it as-it for the time being but really - the UmbracoContext should be injected just
        // like the Services & ApplicationCache properties, and have a setter for those special weird
        // cases.

        // TODO: Can be injected to the view in netcore, else injected to the base model
        // public IUmbracoContext UmbracoContext => _umbracoContext
        //    ?? (_umbracoContext = ViewContext.GetUmbracoContext() ?? Current.UmbracoContext);

        /// <summary>
        /// Gets the public content request.
        /// </summary>
        internal IPublishedRequest PublishedRequest
        {
            get
            {
                // TODO: we only have one data token for a route now: Constants.Web.UmbracoRouteDefinitionDataToken

                throw new NotImplementedException("Probably needs to be ported to netcore");

                //// we should always try to return the object from the data tokens just in case its a custom object and not
                //// the one from UmbracoContext. Fallback to UmbracoContext if necessary.

                //// try view context
                //if (ViewContext.RouteData.DataTokens.ContainsKey(Constants.Web.UmbracoRouteDefinitionDataToken))
                //{
                //    return (IPublishedRequest) ViewContext.RouteData.DataTokens.GetRequiredObject(Constants.Web.UmbracoRouteDefinitionDataToken);
                //}

                //// child action, try parent view context
                //if (ViewContext.IsChildAction && ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey(Constants.Web.UmbracoRouteDefinitionDataToken))
                //{
                //    return (IPublishedRequest) ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject(Constants.Web.UmbracoRouteDefinitionDataToken);
                //}

                //// fallback to UmbracoContext
                //return UmbracoContext.PublishedRequest;
            }
        }

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco
        {
            get
            {
                if (_helper != null) return _helper;

                var model = ViewData.Model;
                var content = model as IPublishedContent;
                if (content == null && model is IContentModel)
                    content = ((IContentModel) model).Content;

                _helper = Current.UmbracoHelper;

                if (content != null)
                    _helper.AssignedContentItem = content;

                return _helper;
            }
        }

        protected UmbracoViewPage()
            : this(
                Current.Factory.GetRequiredService<ServiceContext>(),
                Current.Factory.GetRequiredService<AppCaches>(),
                Current.Factory.GetRequiredService<IOptions<GlobalSettings>>(),
                Current.Factory.GetRequiredService<IOptions<ContentSettings>>()
            )
        {
        }

        protected UmbracoViewPage(ServiceContext services, AppCaches appCaches, IOptions<GlobalSettings> globalSettings, IOptions<ContentSettings> contentSettings)
        {
            if (globalSettings == null) throw new ArgumentNullException(nameof(globalSettings));
            if (contentSettings == null) throw new ArgumentNullException(nameof(contentSettings));
            Services = services;
            AppCaches = appCaches;
            _globalSettings = globalSettings.Value;
            _contentSettings = contentSettings.Value;
        }

    }
}
