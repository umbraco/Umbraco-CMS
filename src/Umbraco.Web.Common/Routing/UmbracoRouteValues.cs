using System;
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
            IPublishedContent publishedContent,
            string controllerName = null,
            Type controllerType = null,
            string actionName = DefaultActionName,
            string templateName = null,
            bool hasHijackedRoute = false)
        {
            ControllerName = controllerName ?? ControllerExtensions.GetControllerName<RenderController>();
            ControllerType = controllerType ?? typeof(RenderController);
            PublishedContent = publishedContent;
            HasHijackedRoute = hasHijackedRoute;
            ActionName = actionName;
            TemplateName = templateName;
        }

        /// <summary>
        /// Gets the controller name
        /// </summary>
        public string ControllerName { get; }

        /// <summary>
        /// Gets the action name
        /// </summary>
        public string ActionName { get; }

        /// <summary>
        /// Gets the template name
        /// </summary>
        public string TemplateName { get; }

        /// <summary>
        /// Gets the Controller type found for routing to
        /// </summary>
        public Type ControllerType { get; }

        /// <summary>
        /// Gets the <see cref="IPublishedContent"/>
        /// </summary>
        public IPublishedContent PublishedContent { get; }

        /// <summary>
        /// Gets a value indicating whether the current request has a hijacked route/user controller routed for it
        /// </summary>
        public bool HasHijackedRoute { get; }
    }
}
