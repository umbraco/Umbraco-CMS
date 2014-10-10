using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;
using Umbraco.Core;
using Language = umbraco.cms.businesslogic.language.Language;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Used for custom routed pages that are being integrated with the Umbraco data but are not
    /// part of the umbraco request pipeline. This allows umbraco macros to be able to execute in this scenario.
    /// </summary>
    /// <remarks>
    /// This is inspired from this discussion:
    /// http://our.umbraco.org/forum/developers/extending-umbraco/41367-Umbraco-6-MVC-Custom-MVC-Route?p=3
    /// 
    /// which is based on custom routing found here:
    /// http://shazwazza.com/post/Custom-MVC-routing-in-Umbraco
    /// </remarks>
    public class EnsurePublishedContentRequestAttribute : ActionFilterAttribute
    {
        private readonly string _dataTokenName;
        private readonly string _culture;
        private UmbracoContext _umbracoContext;
        private readonly int? _contentId;
        private UmbracoHelper _helper;

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="contentId"></param>
        /// <param name="culture"></param>
        public EnsurePublishedContentRequestAttribute(UmbracoContext umbracoContext, int contentId, string culture = null)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _contentId = contentId;
            _culture = culture;
        }

        /// <summary>
        /// A constructor used to set an explicit content Id to the PublishedContentRequest that will be created
        /// </summary>
        /// <param name="contentId"></param>
        public EnsurePublishedContentRequestAttribute(int contentId)
        {
            _contentId = contentId;
        }

        /// <summary>
        /// A constructor used to set the data token key name that contains a reference to a PublishedContent instance
        /// </summary>
        /// <param name="dataTokenName"></param>
        public EnsurePublishedContentRequestAttribute(string dataTokenName)
        {
            _dataTokenName = dataTokenName;
        }

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="dataTokenName"></param>
        /// <param name="culture"></param>
        public EnsurePublishedContentRequestAttribute(UmbracoContext umbracoContext, string dataTokenName, string culture = null)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _dataTokenName = dataTokenName;
            _culture = culture;
        }

        /// <summary>
        /// Exposes the UmbracoContext
        /// </summary>
        protected UmbracoContext UmbracoContext
        {
            get { return _umbracoContext ?? (_umbracoContext = UmbracoContext.Current); }
        }

        /// <summary>
        /// Exposes an UmbracoHelper
        /// </summary>
        protected UmbracoHelper Umbraco
        {
            get { return _helper ?? (_helper = new UmbracoHelper(UmbracoContext.Current)); }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            //First we need to check if the pcr has been set, if it has we're going to ignore this and not actually do anything
            if (UmbracoContext.Current.PublishedContentRequest != null)
            {
                return;
            }

            UmbracoContext.Current.PublishedContentRequest =
                new PublishedContentRequest(
                    UmbracoContext.Current.CleanedUmbracoUrl, UmbracoContext.Current.RoutingContext);

            ConfigurePublishedContentRequest(UmbracoContext.Current.PublishedContentRequest, filterContext);
        }

        /// <summary>
        /// This assigns the published content to the request, developers can override this to specify 
        /// any other custom attributes required.
        /// </summary>
        /// <param name="pcr"></param>
        /// <param name="filterContext"></param>
        protected virtual void ConfigurePublishedContentRequest(PublishedContentRequest pcr, ActionExecutedContext filterContext)
        {
            if (_contentId.HasValue)
            {
                var content = Umbraco.TypedContent(_contentId);
                if (content == null)
                {
                    throw new InvalidOperationException("Could not resolve content with id " + _contentId);
                }
                pcr.PublishedContent = content;
            }
            else if (_dataTokenName.IsNullOrWhiteSpace() == false)
            {
                var result = filterContext.RouteData.DataTokens[_dataTokenName];
                if (result == null)
                {
                    throw new InvalidOperationException("No data token could be found with the name " + _dataTokenName);
                }
                if ((result is IPublishedContent) == false)
                {
                    throw new InvalidOperationException("The data token resolved with name " + _dataTokenName + " was not an instance of " + typeof(IPublishedContent));
                }
                pcr.PublishedContent = (IPublishedContent) result;
            }

            pcr.Prepare();
        }

    }
}