// Copyright (c) Umbraco.

namespace Umbraco.Cms.Core.Notifications;

public abstract class StatefulNotification : IStatefulNotification
{
    private IDictionary<string, object?>? _state;

    /// <summary>
    ///     This can be used by event subscribers to store state in the notification so they easily deal with custom state data
    ///     between a starting ("ing") and an ending ("ed") notification
    /// </summary>
    public IDictionary<string, object?> State
    {
        get => _state ??= new Dictionary<string, object?>();
        set => _state = value;
    }
}
