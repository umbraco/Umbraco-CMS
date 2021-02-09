using System;
using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.Routing
{
    /// <summary>
    /// Represents the data required to route to a specific controller/action during an Umbraco request
    /// </summary>
    public class UmbracoRouteValues
    {
        /// <summary>
        /// The default action name
        /// </summary>
        public const string DefaultActionName = nameof(RenderController.Index);

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRouteValues"/> class.
        /// </summary>
        public UmbracoRouteValues(
            IPublishedRequest publishedRequest,
            ControllerActionDescriptor controllerActionDescriptor,
            string templateName = null,
            bool hasHijackedRoute = false)
        {
            PublishedRequest = publishedRequest;
            ControllerActionDescriptor = controllerActionDescriptor;
            HasHijackedRoute = hasHijackedRoute;
            TemplateName = templateName;
        }

        /// <summary>
        /// Gets the controller name
        /// </summary>
        public string ControllerName => ControllerActionDescriptor.ControllerName;

        /// <summary>
        /// Gets the action name
        /// </summary>
        public string ActionName => ControllerActionDescriptor.ActionName;

        /// <summary>
        /// Gets the template name
        /// </summary>
        public string TemplateName { get; }

        /// <summary>
        /// Gets the controller type
        /// </summary>
        public Type ControllerType => ControllerActionDescriptor.ControllerTypeInfo;

        /// <summary>
        /// Gets the Controller descriptor found for routing to
        /// </summary>
        public ControllerActionDescriptor ControllerActionDescriptor { get; }

        /// <summary>
        /// Gets the <see cref="IPublishedRequest"/>
        /// </summary>
        public IPublishedRequest PublishedRequest { get; }

        /// <summary>
        /// Gets a value indicating whether the current request has a hijacked route/user controller routed for it
        /// </summary>
        public bool HasHijackedRoute { get; }
    }
}
