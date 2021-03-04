// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Events
{
    public interface IScopedNotificationPublisher
    {
        /// <summary>
        /// Publishes a cancelable notification to the notification subscribers
        /// </summary>
        /// <param name="notification"></param>
        /// <returns>True if the notification was cancelled by a subscriber, false otherwise</returns>
        bool PublishCancelable(ICancelableNotification notification);

        /// <summary>
        /// Publishes a cancelable notification to the notification subscribers
        /// </summary>
        /// <param name="notification"></param>
        /// <returns>True if the notification was cancelled by a subscriber, false otherwise</returns>
        Task<bool> PublishCancelableAsync(ICancelableNotification notification);

        /// <summary>
        /// Publishes a notification to the notification subscribers
        /// </summary>
        /// <param name="notification"></param>
        /// <remarks>The notification is published upon successful completion of the current scope, i.e. when things have been saved/published/deleted etc.</remarks>
        void Publish(INotification notification);

        /// <summary>
        /// Invokes publishing of all pending notifications within the current scope
        /// </summary>
        /// <param name="completed"></param>
        void ScopeExit(bool completed);
    }
}
