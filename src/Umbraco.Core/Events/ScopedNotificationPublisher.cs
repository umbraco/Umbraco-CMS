// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

public class ScopedNotificationPublisher : ScopedNotificationPublisher<INotificationHandler>
{
    public ScopedNotificationPublisher(IEventAggregator eventAggregator)
        : base(eventAggregator)
    { }
}

public class ScopedNotificationPublisher<TNotificationHandler> : IScopedNotificationPublisher
    where TNotificationHandler : INotificationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly List<INotification> _notificationOnScopeCompleted = new List<INotification>();
    private readonly bool _publishCancelableNotificationOnScopeExit;
    private readonly Lock _locker = new();
    private bool _isSuppressed;

    public ScopedNotificationPublisher(IEventAggregator eventAggregator, bool publishCancelableNotificationOnScopeExit = false)
    {
        _eventAggregator = eventAggregator;
        _publishCancelableNotificationOnScopeExit |= publishCancelableNotificationOnScopeExit;
    }

    public bool PublishCancelable(ICancelableNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (_isSuppressed)
        {
            return false;
        }

        if (_publishCancelableNotificationOnScopeExit)
        {
            _notificationOnScopeCompleted.Add(notification);
        }
        else
        {
            _eventAggregator.Publish(notification);
        }

        return notification.Cancel;
    }

    public async Task<bool> PublishCancelableAsync(ICancelableNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (_isSuppressed)
        {
            return false;
        }

        if (_publishCancelableNotificationOnScopeExit)
        {
            _notificationOnScopeCompleted.Add(notification);
        }
        else
        {
            Task task = _eventAggregator.PublishAsync(notification);
            if (task is not null)
            {
                await task;
            }
        }

        return notification.Cancel;
    }

    public void Publish(INotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

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
                PublishScopedNotifications(_notificationOnScopeCompleted);
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
                throw new InvalidOperationException("Notifications are already suppressed.");
            }

            return new Suppressor(this);
        }
    }

    protected virtual void PublishScopedNotifications(IList<INotification> notifications)
        => _eventAggregator.Publish<INotification, TNotificationHandler>(notifications);

    private class Suppressor : IDisposable
    {
        private readonly ScopedNotificationPublisher<TNotificationHandler> _scopedNotificationPublisher;
        private bool _disposedValue;

        public Suppressor(ScopedNotificationPublisher<TNotificationHandler> scopedNotificationPublisher)
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
