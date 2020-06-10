using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    internal static class ControllerFactoryExtensions
    {
        /// <summary>
        /// Gets a controller type by the name
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="requestContext"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is related to issue: http://issues.umbraco.org/issue/U4-1726. We already have a method called GetControllerTypeInternal on our MasterControllerFactory,
        /// however, we cannot always guarantee that the usage of this will be a MasterControllerFactory like during unit tests. So we needed to create
        /// this extension method to do the checks instead.
        /// </remarks>
        internal static Type GetControllerTypeInternal(this IControllerFactory factory, RequestContext requestContext, string controllerName)
        {
            if (factory is MasterControllerFactory controllerFactory)
                return controllerFactory.GetControllerTypeInternal(requestContext, controllerName);

            //we have no choice but to instantiate the controller
            var instance = factory.CreateController(requestContext, controllerName);
            var controllerType = instance?.GetType();
            factory.ReleaseController(instance);

            return controllerType;
        }
    }
}
