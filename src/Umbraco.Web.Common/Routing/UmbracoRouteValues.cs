using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.Common.Routing;

/// <summary>
///     Represents the data required to route to a specific controller/action during an Umbraco request
/// </summary>
public class UmbracoRouteValues
{
    /// <summary>
    ///     The default action name
    /// </summary>
    public const string DefaultActionName = nameof(RenderController.Index);

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRouteValues" /> class.
    /// </summary>
    public UmbracoRouteValues(
        IPublishedRequest publishedRequest,
        ControllerActionDescriptor controllerActionDescriptor,
        string? templateName = null)
    {
        PublishedRequest = publishedRequest;
        ControllerActionDescriptor = controllerActionDescriptor;
        TemplateName = templateName;
    }

    /// <summary>
    ///     Gets the controller name
    /// </summary>
    public string ControllerName => ControllerActionDescriptor.ControllerName;

    /// <summary>
    ///     Gets the action name
    /// </summary>
    public string ActionName => ControllerActionDescriptor.ActionName;

    /// <summary>
    ///     Gets the template name
    /// </summary>
    public string? TemplateName { get; }

    /// <summary>
    ///     Gets the controller type
    /// </summary>
    public Type ControllerType => ControllerActionDescriptor.ControllerTypeInfo;

    /// <summary>
    ///     Gets the Controller descriptor found for routing to
    /// </summary>
    public ControllerActionDescriptor ControllerActionDescriptor { get; }

    /// <summary>
    ///     Gets the <see cref="IPublishedRequest" />
    /// </summary>
    public IPublishedRequest PublishedRequest { get; }
}
