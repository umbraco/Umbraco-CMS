using System;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class ContainerControllerFactory : DefaultControllerFactory
    {
        private readonly IFactory _container;

        public ContainerControllerFactory(IFactory container)
        {
            _container = container;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            try
            {
                return (IController) _container.GetInstance(controllerType);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create an instance of controller type {controllerType.FullName} (see inner exception).", e);
            }
        }

        public override void ReleaseController(IController controller)
        {
            _container.Release(controller);
        }
    }
}
