using System;

namespace Umbraco.Web.Mvc
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public static string GetControllerName(Type controllerType)
        {
            return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetControllerName<T>()
        {
            return GetControllerName(typeof(T));
        }
    }
}
