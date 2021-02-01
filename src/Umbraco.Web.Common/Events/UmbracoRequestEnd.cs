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
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestEnd"/> class.
        /// </summary>
        public UmbracoRequestEnd(HttpContext httpContext) => HttpContext = httpContext;

        /// <summary>
        /// Gets the <see cref="HttpContext"/>
        /// </summary>
        public HttpContext HttpContext { get; }
    }
}
