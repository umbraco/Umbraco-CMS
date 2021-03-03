using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Events
{
    public class ScopedNotificationPublisher : IScopedNotificationPublisher
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly List<INotification> _notificationOnScopeCompleted;

        public ScopedNotificationPublisher(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _notificationOnScopeCompleted = new List<INotification>();
        }

        public bool PublishCancelable(ICancelableNotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _eventAggregator.Publish(notification);
            return notification.Cancel;
        }

        public async Task<bool> PublishCancelableAsync(ICancelableNotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            await _eventAggregator.PublishAsync(notification);
            return notification.Cancel;
        }

        public void Publish(INotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _notificationOnScopeCompleted.Add(notification);
        }

        public void ScopeExit(bool completed)
        {
            try
            {
                if (completed)
                {
                    foreach (var notification in _notificationOnScopeCompleted)
                    {
                        _eventAggregator.Publish(notification);
                    }
                }
            }
            finally
            {
                _notificationOnScopeCompleted.Clear();
            }
        }
    }
}
