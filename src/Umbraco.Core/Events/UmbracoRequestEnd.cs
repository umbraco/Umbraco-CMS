// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Events
{
    /// <summary>
    /// Notification raised on each request end.
    /// </summary>
    public class UmbracoRequestEnd : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestEnd"/> class.
        /// </summary>
        public UmbracoRequestEnd(IUmbracoContext umbracoContext) => UmbracoContext = umbracoContext;

        /// <summary>
        /// Gets the <see cref="IUmbracoContext"/>
        /// </summary>
        public IUmbracoContext UmbracoContext { get; }
    }
}
