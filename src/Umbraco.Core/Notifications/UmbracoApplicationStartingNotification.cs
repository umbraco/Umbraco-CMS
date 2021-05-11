// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications
{

    public class UmbracoApplicationStartingNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStartingNotification"/> class.
        /// </summary>
        /// <param name="runtimeLevel">The runtime level</param>
        public UmbracoApplicationStartingNotification(RuntimeLevel runtimeLevel) => RuntimeLevel = runtimeLevel;

        /// <summary>
        /// Gets the runtime level of execution.
        /// </summary>
        public RuntimeLevel RuntimeLevel { get; }
    }
}
