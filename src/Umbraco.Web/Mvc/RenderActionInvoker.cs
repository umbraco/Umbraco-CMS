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
	public class CustomActionInvoker : AsyncControllerActionInvoker
    {
        private static readonly ConcurrentDictionary<Type, ActionDescriptor> IndexDescriptors = new ConcurrentDictionary<Type, ActionDescriptor>(); 

        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var ad = base.FindAction(controllerContext, controllerDescriptor, actionName);

            if (ad == null)
            {
                if (controllerContext.Controller is IRenderMvcController)
                {
                    return IndexDescriptors.GetOrAdd(controllerContext.Controller.GetType(), type =>
                    {
                        var methodInfo = controllerContext.Controller.GetType().GetMethods()
                            .First(x => x.Name == "Index" &&
                                        x.GetCustomAttributes(typeof (NonActionAttribute), false).Any() == false);

                        return typeof (Task).IsAssignableFrom(methodInfo.ReturnType)
                            ? new TaskAsyncActionDescriptor(methodInfo, "Index", controllerDescriptor)
                            : (ActionDescriptor) new ReflectedActionDescriptor(methodInfo, "Index", controllerDescriptor);
                    });
                }
            }

            return ad;
        }
    }

}