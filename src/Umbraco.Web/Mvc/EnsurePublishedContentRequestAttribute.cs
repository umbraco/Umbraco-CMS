using System;
using System.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Used for custom routed pages that are being integrated with the Umbraco data but are not
    /// part of the umbraco request pipeline. This allows umbraco macros to be able to execute in this scenario.
    /// </summary>
    /// <remarks>
    /// This is inspired from this discussion:
    /// https://our.umbraco.com/forum/developers/extending-umbraco/41367-Umbraco-6-MVC-Custom-MVC-Route?p=3
    ///
    /// which is based on custom routing found here:
    /// http://shazwazza.com/post/Custom-MVC-routing-in-Umbraco
    /// </remarks>
    public class EnsurePublishedContentRequestAttribute : ActionFilterAttribute
    {
        private readonly string _dataTokenName;
        private IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly int? _contentId;
        private UmbracoHelper _helper;

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="contentId"></param>
        public EnsurePublishedContentRequestAttribute(IUmbracoContextAccessor umbracoContextAccessor, int contentId)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _contentId = contentId;
        }

        /// <summary>
        /// A constructor used to set an explicit content Id to the PublishedRequest that will be created
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
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="dataTokenName"></param>
        public EnsurePublishedContentRequestAttribute(IUmbracoContextAccessor umbracoContextAccessor, string dataTokenName)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _dataTokenName = dataTokenName;
        }

        /// <summary>
        /// Exposes the UmbracoContext
        /// </summary>
        protected UmbracoContext UmbracoContext => _umbracoContextAccessor?.UmbracoContext ?? Current.UmbracoContext;

        // TODO: try lazy property injection?
        private IPublishedRouter PublishedRouter => Core.Composing.Current.Factory.GetInstance<IPublishedRouter>();

        /// <summary>
        /// Exposes an UmbracoHelper
        /// </summary>
        protected UmbracoHelper Umbraco => _helper ?? (_helper = Current.Factory.GetInstance<UmbracoHelper>());

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            // First we need to check if the published content request has been set, if it has we're going to ignore this and not actually do anything
            if (Current.UmbracoContext.PublishedRequest != null)
            {
                return;
            }

            Current.UmbracoContext.PublishedRequest = PublishedRouter.CreateRequest(Current.UmbracoContext);
            ConfigurePublishedContentRequest(Current.UmbracoContext.PublishedRequest, filterContext);
        }

        /// <summary>
        /// This assigns the published content to the request, developers can override this to specify
        /// any other custom attributes required.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterContext"></param>
        protected virtual void ConfigurePublishedContentRequest(PublishedRequest request, ActionExecutedContext filterContext)
        {
            if (_contentId.HasValue)
            {
                var content = Umbraco.Content(_contentId.Value);
                if (content == null)
                {
                    throw new InvalidOperationException("Could not resolve content with id " + _contentId);
                }
                request.PublishedContent = content;
            }
            else if (_dataTokenName.IsNullOrWhiteSpace() == false)
            {
                var result = filterContext.RouteData.DataTokens[_dataTokenName];
                if (result == null)
                {
                    throw new InvalidOperationException("No data token could be found with the name " + _dataTokenName);
                }
                if (result is IPublishedContent == false)
                {
                    throw new InvalidOperationException("The data token resolved with name " + _dataTokenName + " was not an instance of " + typeof(IPublishedContent));
                }
                request.PublishedContent = (IPublishedContent)result;
            }

            PublishedRouter.PrepareRequest(request);
        }
    }
}
