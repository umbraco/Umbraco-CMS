// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs during the Umbraco boot process, before instances of <see cref="IComponent"/> initialize.
    /// </summary>
    public class UmbracoApplicationComponentsInstallingNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStartingNotification"/> class.
        /// </summary>
        /// <param name="runtimeLevel">The runtime level</param>
        public UmbracoApplicationComponentsInstallingNotification(RuntimeLevel runtimeLevel) => RuntimeLevel = runtimeLevel;

        /// <summary>
        /// Gets the runtime level of execution.
        /// </summary>
        public RuntimeLevel RuntimeLevel { get; }
    }
}
