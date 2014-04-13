using System.Linq;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
	/// </summary>
	public class RenderActionInvoker : ControllerActionInvoker
	{

		/// <summary>
		/// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="controllerDescriptor"></param>
		/// <param name="actionName"></param>
		/// <returns></returns>
		protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
		{
			var ad = base.FindAction(controllerContext, controllerDescriptor, actionName);

			//now we need to check if it exists, if not we need to return the Index by default
			if (ad == null)
			{
                //check if the controller is an instance of IRenderMvcController
				if (controllerContext.Controller is IRenderMvcController)
				{
				    return new ReflectedActionDescriptor(
				        controllerContext.Controller.GetType().GetMethods()
				            .First(x => x.Name == "Index" &&
				                        x.GetCustomAttributes(typeof (NonActionAttribute), false).Any() == false),
				        "Index",
				        controllerDescriptor);

				}
			}
			return ad;
		}

	}
}