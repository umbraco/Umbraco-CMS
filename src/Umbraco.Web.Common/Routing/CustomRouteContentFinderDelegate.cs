using System;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Common.Extensions
{
    internal class CustomRouteContentFinderDelegate
    {
        private readonly Func<HttpContext, IPublishedContent> _findContent;

        public CustomRouteContentFinderDelegate(Func<HttpContext, IPublishedContent> findContent) => _findContent = findContent;

        public IPublishedContent FindContent(HttpContext httpContext) => _findContent(httpContext);
    }
}
