using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Used for custom routed controllers to execute within the context of Umbraco
/// </summary>
public interface IVirtualPageController
{
    /// <summary>
    ///     Returns the <see cref="IPublishedContent" /> to use as the current page for the request
    /// </summary>
    IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext);
}
