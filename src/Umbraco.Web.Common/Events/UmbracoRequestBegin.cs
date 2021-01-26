// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Notification raised on each request begin.
    /// </summary>
    public class UmbracoRequestBegin : INotification
    {
        public UmbracoRequestBegin(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public HttpContext HttpContext { get; }
    };
}
