using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Umbraco.Core;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Abstract filtered controller factory used for all Umbraco controller factory implementations
    /// </summary>
    public abstract class UmbracoControllerFactory : IFilteredControllerFactory
    {
        private readonly OverridenDefaultControllerFactory _innerFactory = new OverridenDefaultControllerFactory();

        public abstract bool CanHandle(RequestContext request);

        public virtual Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            return _innerFactory.GetControllerType(requestContext, controllerName);
        }

        /// <summary>
        /// Creates the specified controller by using the specified request context.
        /// </summary>
        /// <returns>
        /// The controller.
        /// </returns>
        /// <param name="requestContext">The request context.</param><param name="controllerName">The name of the controller.</param>
        public virtual IController CreateController(RequestContext requestContext, string controllerName)
        {
            var controllerType = GetControllerType(requestContext, controllerName) ??
                _innerFactory.GetControllerType(
                    requestContext,
                    ControllerExtensions.GetControllerName(Current.DefaultRenderMvcControllerType));

            return _innerFactory.GetControllerInstance(requestContext, controllerType);
        }

        /// <summary>
        /// Gets the controller's session behavior.
        /// </summary>
        /// <returns>
        /// The controller's session behavior.
        /// </returns>
        /// <param name="requestContext">The request context.</param><param name="controllerName">The name of the controller whose session behavior you want to get.</param>
        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return ((IControllerFactory)_innerFactory).GetControllerSessionBehavior(requestContext, controllerName);
        }

        /// <summary>
        /// Releases the specified controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public virtual void ReleaseController(IController controller)
        {
            _innerFactory.ReleaseController(controller);
        }

        /// <summary>
        /// By default, <see cref="DefaultControllerFactory"/> only exposes <see cref="IControllerFactory.CreateController"/> which throws an exception
        /// if the controller is not found. Since we want to try creating a controller, and then fall back to <see cref="RenderMvcController"/> if one isn't found,
        /// this nested class changes the visibility of <see cref="DefaultControllerFactory"/>'s internal methods in order to not have to rely on a try-catch.
        /// </summary>
        /// <remarks></remarks>
        internal class OverridenDefaultControllerFactory : ContainerControllerFactory
        {
            public OverridenDefaultControllerFactory()
                : base(Current.Factory)
            { }

            public new IController GetControllerInstance(RequestContext requestContext, Type controllerType)
            {
                return base.GetControllerInstance(requestContext, controllerType);
            }

            public new Type GetControllerType(RequestContext requestContext, string controllerName)
            {
                return controllerName.IsNullOrWhiteSpace()
                    ? null
                    : base.GetControllerType(requestContext, controllerName);
            }
        }
    }
}
