using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    /// <summary>
	/// A controller factory which uses an internal list of <see cref="IFilteredControllerFactory"/> in order to invoke 
	/// different controller factories dependent upon their implementation of <see cref="IFilteredControllerFactory.CanHandle"/> for the current
	/// request. Allows circumvention of MVC3's singly registered IControllerFactory.
	/// </summary>
	/// <remarks></remarks>
	internal class MasterControllerFactory : DefaultControllerFactory
	{
		private readonly FilteredControllerFactoriesResolver _slaveFactories;

		public MasterControllerFactory(FilteredControllerFactoriesResolver factoryResolver)
		{
			_slaveFactories = factoryResolver;
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
			var factory = _slaveFactories.Factories.FirstOrDefault(x => x.CanHandle(requestContext));
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
            var factory = _slaveFactories.Factories.FirstOrDefault(x => x.CanHandle(requestContext));
            if (factory != null)
            {
                //check to see if the factory is of type UmbracoControllerFactory which exposes the GetControllerType method so we don't have to create
                // an instance of the controller to figure out what it is. This is a work around for not having a breaking change for:
                // http://issues.umbraco.org/issue/U4-1726

                var umbFactory = factory as UmbracoControllerFactory;
                if (umbFactory != null)
                {
                    return umbFactory.GetControllerType(requestContext, controllerName);
                }
                //we have no choice but to instantiate the controller
                var instance = factory.CreateController(requestContext, controllerName);
                if (instance != null)
                {
                    return instance.GetType();    
                }
                return null;
            }

            return base.GetControllerType(requestContext, controllerName);
        }

		/// <summary>
		/// Releases the specified controller.
		/// </summary>
		/// <param name="controller">The controller to release.</param>
		/// <remarks></remarks>
		public override void ReleaseController(IController controller)
		{
			bool released = false;
				if (controller is Controller)
				{
					var requestContext = ((Controller)controller).ControllerContext.RequestContext;
					var factory = _slaveFactories.Factories.FirstOrDefault(x => x.CanHandle(requestContext));
					if (factory != null)
					{
						factory.ReleaseController(controller);
						released = true;
					}
				}
				if (!released) base.ReleaseController(controller);
		}
	}
}