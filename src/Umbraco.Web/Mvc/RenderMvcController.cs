using System;
using System.IO;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{

    /// <summary>
    /// A controller to render front-end requests
    /// </summary>
    [PreRenderViewActionFilter]
    public class RenderMvcController : UmbracoController, IRenderMvcController
	{

		public RenderMvcController()
            : base()
		{
			ActionInvoker = new RenderActionInvoker();
		}

        public RenderMvcController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
            
        }
		
		private PublishedContentRequest _publishedContentRequest;

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        protected new UmbracoContext UmbracoContext
        {
            get { return PublishedContentRequest.RoutingContext.UmbracoContext; }
        }

        /// <summary>
        /// Returns the Current published content item for rendering the content
        /// </summary>
	    protected IPublishedContent CurrentPage
	    {
	        get { return PublishedContentRequest.PublishedContent; }
	    }

		//TODO: make this protected once we make PublishedContentRequest not internal after we figure out what it should actually contain
		/// <summary>
		/// Returns the current PublishedContentRequest
		/// </summary>
		internal PublishedContentRequest PublishedContentRequest
		{
			get
			{
				if (_publishedContentRequest != null)
					return _publishedContentRequest;
				if (!RouteData.DataTokens.ContainsKey("umbraco-doc-request"))
				{
					throw new InvalidOperationException("DataTokens must contain an 'umbraco-doc-request' key with a PublishedContentRequest object");
				}
				_publishedContentRequest = (PublishedContentRequest) RouteData.DataTokens["umbraco-doc-request"];
				return _publishedContentRequest;
			}
		}
		
		/// <summary>
		/// Checks to make sure the physical view file exists on disk
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		protected bool EnsurePhsyicalViewExists(string template)
		{
            var result = ViewEngines.Engines.FindView(ControllerContext, template, null);
            if(result.View == null)
            {
                LogHelper.Warn<RenderMvcController>("No physical template file was found for template " + template);
                return false;
            }
            return true;
		}

		/// <summary>
		/// Returns an ActionResult based on the template name found in the route values and the given model.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <returns></returns>
		/// <remarks>
		/// If the template found in the route values doesn't physically exist, then an empty ContentResult will be returned.
		/// </remarks>
		protected ActionResult CurrentTemplate<T>(T model)
		{
			var template = ControllerContext.RouteData.Values["action"].ToString();
			if (!EnsurePhsyicalViewExists(template))
			{
				return Content("");
			}
			return View(template, model);
		}

		/// <summary>
		/// The default action to render the front-end view
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public virtual ActionResult Index(RenderModel model)
		{
			return CurrentTemplate(model);
		}

	}
}