using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
	/// </summary>
    public class RenderActionInvoker : AsyncControllerActionInvoker
	{

	    private static readonly ConcurrentDictionary<Type, ActionDescriptor> IndexDescriptors = new ConcurrentDictionary<Type, ActionDescriptor>(); 

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
					var methodInfo = controllerContext.Controller.GetType().GetMethods()
						.First(x => x.Name == "Index" &&
							        x.GetCustomAttributes(typeof (NonActionAttribute), false).Any() == false);

					ad = typeof (Task).IsAssignableFrom(methodInfo.ReturnType)
						? new TaskAsyncActionDescriptor(methodInfo, "Index", controllerDescriptor)
						: (ActionDescriptor) new ReflectedActionDescriptor(methodInfo, "Index", controllerDescriptor);

					return IndexDescriptors.GetOrAdd(controllerContext.Controller.GetType(), type => ad);
				}
			}
			return ad;
		}

	}
}