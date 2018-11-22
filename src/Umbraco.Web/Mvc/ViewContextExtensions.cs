using System.IO;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	internal static class ViewContextExtensions
	{
		/// <summary>
		/// Creates a new ViewContext from an existing one but specifies a new Model for the ViewData
		/// </summary>
		/// <param name="vc"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public static ViewContext CopyWithModel(this ViewContext vc, object model)
		{
			return new ViewContext
				{
					Controller = vc.Controller,
					HttpContext = vc.HttpContext,
					RequestContext = vc.RequestContext,
					RouteData = vc.RouteData,
					TempData = vc.TempData,
					View = vc.View,
					ViewData = new ViewDataDictionary(vc)
						{
							Model = model
						},
					FormContext = vc.FormContext,
					ClientValidationEnabled = vc.ClientValidationEnabled,
					UnobtrusiveJavaScriptEnabled = vc.UnobtrusiveJavaScriptEnabled,
					Writer = vc.Writer
				};
		}

		//public static ViewContext CloneWithWriter(this ViewContext vc, TextWriter writer)
		//{
		//	return new ViewContext
		//	{
		//		Controller = vc.Controller,
		//		HttpContext = vc.HttpContext,
		//		RequestContext = vc.RequestContext,
		//		RouteData = vc.RouteData,
		//		TempData = vc.TempData,
		//		View = vc.View,
		//		ViewData = vc.ViewData,
		//		FormContext = vc.FormContext,
		//		ClientValidationEnabled = vc.ClientValidationEnabled,
		//		UnobtrusiveJavaScriptEnabled = vc.UnobtrusiveJavaScriptEnabled,
		//		Writer = writer
		//	};
		//}
	}
}