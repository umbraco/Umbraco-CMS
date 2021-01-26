// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Notification raised on each request end.
    /// </summary>
    public class UmbracoRequestEnd : INotification
    {
        public UmbracoRequestEnd(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public HttpContext HttpContext { get; }
    }
}
