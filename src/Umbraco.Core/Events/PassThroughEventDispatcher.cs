namespace Umbraco.Cms.Core.Events;

/// <summary>
///     An IEventDispatcher that immediately raise all events.
/// </summary>
/// <remarks>
///     This means that events will be raised during the scope transaction,
///     whatever happens, and the transaction could roll back in the end.
/// </remarks>
internal class PassThroughEventDispatcher : IEventDispatcher
{
    public bool DispatchCancelable(EventHandler? eventHandler, object sender, CancellableEventArgs args, string? eventName = null)
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    public bool DispatchCancelable<TArgs>(EventHandler<TArgs>? eventHandler, object sender, TArgs args, string? eventName = null)
        where TArgs : CancellableEventArgs
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    public bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs>? eventHandler, TSender sender, TArgs args, string? eventName = null)
        where TArgs : CancellableEventArgs
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    public void Dispatch(EventHandler? eventHandler, object sender, EventArgs args, string? eventName = null) =>
        eventHandler?.Invoke(sender, args);

    public void Dispatch<TArgs>(EventHandler<TArgs>? eventHandler, object sender, TArgs args, string? eventName = null) => eventHandler?.Invoke(sender, args);

    public void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs>? eventHandler, TSender sender, TArgs args, string? eventName = null) => eventHandler?.Invoke(sender, args);

    public IEnumerable<IEventDefinition> GetEvents(EventDefinitionFilter filter) =>
        Enumerable.Empty<IEventDefinition>();

    public void ScopeExit(bool completed)
    {
    }
}
