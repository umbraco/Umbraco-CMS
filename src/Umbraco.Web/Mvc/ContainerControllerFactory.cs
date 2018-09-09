using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class ContainerControllerFactory : DefaultControllerFactory
    {
        private readonly IContainer container;

        public ContainerControllerFactory(IContainer container)
        {
            this.container = container;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return (IController)container.GetInstance(controllerType);
        }

        public override void ReleaseController(IController controller)
        {
            container.Release(controller);
        }
    }
}
