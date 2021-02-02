// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Web;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Notification raised on each request begin.
    /// </summary>
    public class UmbracoRequestBegin : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestBegin"/> class.
        /// </summary>
        public UmbracoRequestBegin(IUmbracoContext umbracoContext) => UmbracoContext = umbracoContext;

        /// <summary>
        /// Gets the <see cref="IUmbracoContext"/>
        /// </summary>
        public IUmbracoContext UmbracoContext { get; }
    }
}
