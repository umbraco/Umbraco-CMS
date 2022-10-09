// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

public class ScopedNotificationPublisher : IScopedNotificationPublisher
{
    private readonly IEventAggregator _eventAggregator;
    private readonly object _locker = new();
    private readonly List<INotification> _notificationOnScopeCompleted;
    private bool _isSuppressed;

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

        Task task = _eventAggregator.PublishAsync(notification);
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
        lock (_locker)
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
        private readonly ScopedNotificationPublisher _scopedNotificationPublisher;
        private bool _disposedValue;

        public Suppressor(ScopedNotificationPublisher scopedNotificationPublisher)
        {
            _scopedNotificationPublisher = scopedNotificationPublisher;
            _scopedNotificationPublisher._isSuppressed = true;
        }

        public void Dispose() => Dispose(true);

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
    }
}
