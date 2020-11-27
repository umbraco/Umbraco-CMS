using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A controller factory which uses an internal list of <see cref="IFilteredControllerFactory"/> in order to invoke
    /// different controller factories dependent upon their implementation of <see cref="IFilteredControllerFactory.CanHandle"/> for the current
    /// request. Allows circumvention of MVC3's singly registered IControllerFactory.
    /// </summary>
    /// <remarks></remarks>
    internal class MasterControllerFactory : ContainerControllerFactory
    {
        private readonly Func<FilteredControllerFactoryCollection> _factoriesAccessor;
        private FilteredControllerFactoryCollection _factories;

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterControllerFactory"/> with a factories accessor.
        /// </summary>
        /// <param name="factoriesAccessor">The factories accessor.</param>
        public MasterControllerFactory(Func<FilteredControllerFactoryCollection> factoriesAccessor)
            : base(Current.Factory)
        {
            // note
            // because the MasterControllerFactory needs to be ctored to be assigned to
            // ControllerBuilder.Current when setting up Mvc and WebApi, it cannot be ctored by
            // the IoC container - and yet we don't want that ctor to resolve the factories
            // as that happen before everything is configured - so, passing a factories
            // accessor function.

            _factoriesAccessor = factoriesAccessor;
        }

        private IFilteredControllerFactory FactoryForRequest(RequestContext context)
        {
            if (_factories == null) _factories = _factoriesAccessor();
            return _factories.FirstOrDefault(x => x.CanHandle(context));
        }

        /// <summary>
        /// Creates the specified controller by using the specified request context.
        /// </summary>
        /// <param name="requestContext">The context of the HTTP request, which includes the HTTP context and route data.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>The controller.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="requestContext"/> parameter is null.</exception>
        ///
        /// <exception cref="T:System.ArgumentException">The <paramref name="controllerName"/> parameter is null or empty.</exception>
        /// <remarks></remarks>
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            var factory = FactoryForRequest(requestContext);
            return factory != null
               ? factory.CreateController(requestContext, controllerName)
               : base.CreateController(requestContext, controllerName);
        }

        /// <summary>
        /// Retrieves the controller type for the specified name and request context.
        /// </summary>
        ///
        /// <returns>
        /// The controller type.
        /// </returns>
        /// <param name="requestContext">The context of the HTTP request, which includes the HTTP context and route data.</param>
        /// <param name="controllerName">The name of the controller.</param>
        internal Type GetControllerTypeInternal(RequestContext requestContext, string controllerName)
        {
            var factory = FactoryForRequest(requestContext);
            if (factory != null)
            {
                //check to see if the factory is of type UmbracoControllerFactory which exposes the GetControllerType method so we don't have to create
                // an instance of the controller to figure out what it is. This is a work around for not having a breaking change for:
                // http://issues.umbraco.org/issue/U4-1726

                if (factory is UmbracoControllerFactory umbFactory)
                    return umbFactory.GetControllerType(requestContext, controllerName);

                //we have no choice but to instantiate the controller
                var instance = factory.CreateController(requestContext, controllerName);
                var controllerType = instance?.GetType();
                factory.ReleaseController(instance);

                return controllerType;
            }

            return GetControllerType(requestContext, controllerName);
        }

        /// <summary>
        /// Releases the specified controller.
        /// </summary>
        /// <param name="icontroller">The controller to release.</param>
        /// <remarks></remarks>
        public override void ReleaseController(IController icontroller)
        {
            var released = false;

            if (icontroller is Controller controller)
            {
                var requestContext = controller.ControllerContext.RequestContext;
                var factory = FactoryForRequest(requestContext);
                if (factory != null)
                {
                    factory.ReleaseController(controller);
                    released = true;
                }
            }

            if (released == false)
                base.ReleaseController(icontroller);
        }
    }
}
