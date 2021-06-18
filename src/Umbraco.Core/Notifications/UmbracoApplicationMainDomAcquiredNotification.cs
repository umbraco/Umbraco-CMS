// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs during Umbraco boot after the MainDom has been acquired.
    /// </summary>
    public class UmbracoApplicationMainDomAcquiredNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationMainDomAcquiredNotification"/> class.
        /// </summary>
        /// <param name="runtimeLevel">The runtime level</param>
        public UmbracoApplicationMainDomAcquiredNotification()
        {
        }
    }
}
