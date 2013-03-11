using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The View that umbraco front-end views inherit from
    /// </summary>
    public abstract class UmbracoViewPage<T> : WebViewPage<T>
    {
        protected UmbracoViewPage()
        {

        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext
        {
            get
            {
                //we should always try to return the context from the data tokens just in case its a custom context and not 
                //using the UmbracoContext.Current.
                //we will fallback to the singleton if necessary.
                if (ViewContext.RouteData.DataTokens.ContainsKey("umbraco-context"))
                {
                    return (UmbracoContext)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-context");
                }
                //next check if it is a child action and see if the parent has it set in data tokens
                if (ViewContext.IsChildAction)
                {
                    if (ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey("umbraco-context"))
                    {
                        return (UmbracoContext)ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-context");
                    }
                }

                //lastly, we will use the singleton, the only reason this should ever happen is is someone is rendering a page that inherits from this
                //class and are rendering it outside of the normal Umbraco routing process. Very unlikely.
                return UmbracoContext.Current;
            }
        }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        /// <summary>
        /// Returns the current PublishedContentRequest
        /// </summary>
        internal PublishedContentRequest PublishedContentRequest
        {
            get
            {
                //we should always try to return the object from the data tokens just in case its a custom object and not 
                //using the UmbracoContext.Current.
                //we will fallback to the singleton if necessary.
                if (ViewContext.RouteData.DataTokens.ContainsKey("umbraco-doc-request"))
                {
                    return (PublishedContentRequest)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request");
                }
                //next check if it is a child action and see if the parent has it set in data tokens
                if (ViewContext.IsChildAction)
                {
                    if (ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey("umbraco-doc-request"))
                    {
                        return (PublishedContentRequest)ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request");
                    }
                }

                //lastly, we will use the singleton, the only reason this should ever happen is is someone is rendering a page that inherits from this
                //class and are rendering it outside of the normal Umbraco routing process. Very unlikely.
                return UmbracoContext.Current.PublishedContentRequest;
            }
        }

        private UmbracoHelper _helper;

        /// <summary>
        /// Gets an UmbracoHelper
        /// </summary>
        /// <remarks>
        /// This constructs the UmbracoHelper with the content model of the page routed to
        /// </remarks>
        public virtual UmbracoHelper Umbraco
        {
            get { return _helper ?? (_helper = new UmbracoHelper(UmbracoContext)); }
        }

        /// <summary>
        /// Ensure that the current view context is added to the route data tokens so we can extract it if we like
        /// </summary>
        /// <remarks>
        /// Currently this is required by mvc macro engines
        /// </remarks>
        protected override void InitializePage()
        {
            base.InitializePage();
            if (!ViewContext.IsChildAction)
            {
                if (!ViewContext.RouteData.DataTokens.ContainsKey(Constants.DataTokenCurrentViewContext))
                {
                    ViewContext.RouteData.DataTokens.Add(Constants.DataTokenCurrentViewContext, this.ViewContext);
                }
            }

        }

    }
}