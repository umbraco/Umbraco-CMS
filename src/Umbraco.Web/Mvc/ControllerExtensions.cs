using System;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    internal static class ControllerExtensions
    {
        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        internal static string GetControllerName(Type controllerType)
        {
            if (!controllerType.Name.EndsWith("Controller"))
            {
                throw new InvalidOperationException("The controller type " + controllerType + " does not follow conventions, MVC Controller class names must be suffixed with the term 'Controller'");
            }
            return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
        }

        /// <summary>
        /// Return the controller name from the controller instance
        /// </summary>
        /// <param name="controllerInstance"></param>
        /// <returns></returns>
        internal static string GetControllerName(this IController controllerInstance)
        {
            return GetControllerName(controllerInstance.GetType());
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        internal static string GetControllerName<T>()
        {
            return GetControllerName(typeof(T));
        }

    }
}
