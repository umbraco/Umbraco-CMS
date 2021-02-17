using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Common.Extensions
{
    internal class CustomRouteContentFinderDelegate
    {
        private readonly Func<ActionExecutingContext, IPublishedContent> _findContent;

        public CustomRouteContentFinderDelegate(Func<ActionExecutingContext, IPublishedContent> findContent) => _findContent = findContent;

        public IPublishedContent FindContent(ActionExecutingContext actionExecutingContext) => _findContent(actionExecutingContext);
    }
}
