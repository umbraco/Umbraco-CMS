using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	public class RenderModelBinder : IModelBinder
	{

		/// <summary>
		/// Binds the model to a value by using the specified controller context and binding context.
		/// </summary>
		/// <returns>
		/// The bound value.
		/// </returns>
		/// <param name="controllerContext">The controller context.</param><param name="bindingContext">The binding context.</param>
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var requestMatchesType = typeof(RenderModel) == bindingContext.ModelType;

			if (requestMatchesType)
			{
				//get the model from the route data
				if (!controllerContext.RouteData.DataTokens.ContainsKey("umbraco"))
					return null;
				var model = controllerContext.RouteData.DataTokens["umbraco"] as RenderModel;
				return model;
			}
			return null;
		}
	}
}