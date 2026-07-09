// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Default implementation of <see cref="IScopedNotificationPublisher" /> that publishes notifications within a scope.
/// </summary>
public class ScopedNotificationPublisher : ScopedNotificationPublisher<INotificationHandler>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScopedNotificationPublisher" /> class.
    /// </summary>
    /// <param name="eventAggregator">The event aggregator.</param>
    public ScopedNotificationPublisher(IEventAggregator eventAggregator)
        : base(eventAggregator)
    { }
}

/// <summary>
///     Implementation of <see cref="IScopedNotificationPublisher" /> that publishes notifications within a scope for a specific handler type.
/// </summary>
/// <typeparam name="TNotificationHandler">The type of notification handler.</typeparam>
public class ScopedNotificationPublisher<TNotificationHandler> : IScopedNotificationPublisher
    where TNotificationHandler : INotificationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly List<INotification> _notificationOnScopeCompleted = new List<INotification>();
    private readonly bool _publishCancelableNotificationOnScopeExit;
    private readonly Lock _locker = new();
    private bool _isSuppressed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScopedNotificationPublisher{TNotificationHandler}" /> class.
    /// </summary>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="publishCancelableNotificationOnScopeExit">A value indicating whether cancelable notifications should be published on scope exit.</param>
    public ScopedNotificationPublisher(IEventAggregator eventAggregator, bool publishCancelableNotificationOnScopeExit = false)
    {
        _eventAggregator = eventAggregator;
        _publishCancelableNotificationOnScopeExit |= publishCancelableNotificationOnScopeExit;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Publish(INotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (_isSuppressed)
        {
            return;
        }

        _notificationOnScopeCompleted.Add(notification);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    ///     Publishes all pending scoped notifications.
    /// </summary>
    /// <param name="notifications">The notifications to publish.</param>
    protected virtual void PublishScopedNotifications(IList<INotification> notifications)
        => _eventAggregator.Publish<INotification, TNotificationHandler>(notifications);

    /// <summary>
    ///     A disposable class that suppresses notifications until disposed.
    /// </summary>
    private sealed class Suppressor : IDisposable
    {
        private readonly ScopedNotificationPublisher<TNotificationHandler> _scopedNotificationPublisher;
        private bool _disposedValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Suppressor" /> class.
        /// </summary>
        /// <param name="scopedNotificationPublisher">The scoped notification publisher to suppress.</param>
        public Suppressor(ScopedNotificationPublisher<TNotificationHandler> scopedNotificationPublisher)
        {
            _scopedNotificationPublisher = scopedNotificationPublisher;
            _scopedNotificationPublisher._isSuppressed = true;
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        /// <summary>
        ///     Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
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
