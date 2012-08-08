using System;
using System.IO;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
	public class RenderMvcController : Controller
	{

		public RenderMvcController()
		{
			ActionInvoker = new RenderActionInvoker();
		}
		
		private DocumentRequest _documentRequest;

		/// <summary>
		/// Returns the current UmbracoContext
		/// </summary>
		protected UmbracoContext UmbracoContext
		{
			get { return DocumentRequest.UmbracoContext; }
		}

		//TODO: make this protected once we make DocumentRequest not internal after we figure out what it should actually contain
		/// <summary>
		/// Returns the current DocumentRequest
		/// </summary>
		internal DocumentRequest DocumentRequest
		{
			get
			{
				if (_documentRequest != null)
					return _documentRequest;
				if (!RouteData.DataTokens.ContainsKey("umbraco-doc-request"))
				{
					throw new InvalidOperationException("DataTokens must contain an 'umbraco-doc-request' key with a DocumentRequest object");
				}
				_documentRequest = (DocumentRequest) RouteData.DataTokens["umbraco-doc-request"];
				return _documentRequest;
			}
		}
		
		public virtual ActionResult Index(RenderModel model)
		{
			var template = ControllerContext.RouteData.Values["action"].ToString();
			if (!System.IO.File.Exists(
				Path.Combine(Server.MapPath(Constants.ViewLocation), template + ".cshtml")))
			{
				LogHelper.Warn<RenderMvcController>("No physical template file was found for template " + template);
				return Content("");
			}

			return View(template, model);

		}

	}
}