using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Common.Routing;

internal class CustomRouteContentFinderDelegate
{
    private readonly Func<ActionExecutingContext, IPublishedContent> _findContent;

    public CustomRouteContentFinderDelegate(Func<ActionExecutingContext, IPublishedContent> findContent) =>
        _findContent = findContent;

    public IPublishedContent FindContent(ActionExecutingContext actionExecutingContext) =>
        _findContent(actionExecutingContext);
}
