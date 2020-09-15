﻿using System;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    public class ContainerControllerFactory : DefaultControllerFactory
    {
        private readonly Lazy<IServiceProvider> _factory;

        public ContainerControllerFactory(Lazy<IServiceProvider> factory)
        {
            _factory = factory;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            try
            {
                return (IController) _factory.Value.GetInstance(controllerType);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create an instance of controller type {controllerType.FullName} (see inner exception).", e);
            }
        }

        public override void ReleaseController(IController controller)
        {
            //_factory.Value.Release(controller);
        }
    }
}
