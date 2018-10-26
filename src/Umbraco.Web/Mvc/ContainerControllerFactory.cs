using System;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class ContainerControllerFactory : DefaultControllerFactory
    {
        private readonly IContainer _container;

        public ContainerControllerFactory(IContainer container)
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
