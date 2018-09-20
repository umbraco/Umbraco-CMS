using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A controller factory for the render pipeline of Umbraco. This controller factory tries to create a controller with the supplied
    /// name, and falls back to UmbracoController if none was found.
    /// </summary>
    /// <remarks></remarks>
    public class RenderControllerFactory : UmbracoControllerFactory
    {

        /// <summary>
        /// Determines whether this instance can handle the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns><c>true</c> if this instance can handle the specified request; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public override bool CanHandle(RequestContext request)
        {
            var dataToken = request.RouteData.DataTokens["area"];
            return dataToken == null || string.IsNullOrWhiteSpace(dataToken.ToString());
        }

        /// <summary>
        /// Creates the controller
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// We always set the correct ActionInvoker on our custom created controller, this is very important for route hijacking!
        /// </remarks>
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
              var instance = base.CreateController(requestContext, controllerName);
             var controllerInstance = instance as Controller;
             if (controllerInstance != null)
             {
                 //set the action invoker!
                 controllerInstance.ActionInvoker = new RenderActionInvoker();
             }

             return instance;
        }

    }
}
