// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events
{
    public class ScopedNotificationPublisher : IScopedNotificationPublisher
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly List<INotification> _notificationOnScopeCompleted;
        private readonly object _locker = new object();
        private bool _isSuppressed = false;

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

            if (_isSuppressed)
            {
                return false;
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

            if (_isSuppressed)
            {
                return false;
            }

            var task = _eventAggregator.PublishAsync(notification);
            if (task is not null)
            {
                await task;
            }

            return notification.Cancel;
        }

        public void Publish(INotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (_isSuppressed)
            {
                return;
            }

            _notificationOnScopeCompleted.Add(notification);
        }

        public void ScopeExit(bool completed)
        {
            try
            {
                if (completed)
                {
                    foreach (INotification notification in _notificationOnScopeCompleted)
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

        public IDisposable Suppress()
        {
            lock(_locker)
            {
                if (_isSuppressed)
                {
                    throw new InvalidOperationException("Notifications are already suppressed");
                }
                return new Suppressor(this);
            }
        }

        private class Suppressor : IDisposable
        {
            private bool _disposedValue;
            private readonly ScopedNotificationPublisher _scopedNotificationPublisher;

            public Suppressor(ScopedNotificationPublisher scopedNotificationPublisher)
            {
                _scopedNotificationPublisher = scopedNotificationPublisher;
                _scopedNotificationPublisher._isSuppressed = true;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        lock (_scopedNotificationPublisher._locker)
                        {
                            _scopedNotificationPublisher._isSuppressed = false;
                        }
                    }
                    _disposedValue = true;
                }
            }
            public void Dispose() => Dispose(disposing: true);
        }
    }
}
