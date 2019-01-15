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
            return (IController) _container.GetInstance(controllerType);
        }

        public override void ReleaseController(IController controller)
        {
            _container.Release(controller);
        }
    }
}
