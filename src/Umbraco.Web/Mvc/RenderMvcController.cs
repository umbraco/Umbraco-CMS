using System.IO;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	public class RenderMvcController : Controller
	{

		public RenderMvcController()
		{
			ActionInvoker = new RenderActionInvoker();
		}

		public virtual ActionResult Index(RenderModel model)
		{
			var template = ControllerContext.RouteData.Values["action"].ToString();
			if (!System.IO.File.Exists(
				Path.Combine(Server.MapPath(Constants.ViewLocation), template + ".cshtml")))
			{
				return Content("");
			}

			return View(template, model);

		}

	}
}