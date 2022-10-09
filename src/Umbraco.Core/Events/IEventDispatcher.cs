namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Dispatches events from within a scope.
/// </summary>
/// <remarks>
///     <para>
///         The name of the event is auto-magically discovered by matching the sender type, args type, and
///         eventHandler type. If the match is not unique, then the name parameter must be used to specify the
///         name in an explicit way.
///     </para>
///     <para>
///         What happens when an event is dispatched depends on the scope settings. It can be anything from
///         "trigger immediately" to "just ignore". Refer to the scope documentation for more details.
///     </para>
/// </remarks>
public interface IEventDispatcher
{
    // not sure about the Dispatch & DispatchCancelable signatures at all for now
    // nor about the event name thing, etc - but let's keep it like this

    /// <summary>
    ///     Dispatches a cancelable event.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data.</param>
    /// <param name="name">The optional name of the event.</param>
    /// <returns>A value indicating whether the cancelable event was cancelled.</returns>
    /// <remarks>See general remarks on the interface.</remarks>
    bool DispatchCancelable(EventHandler eventHandler, object sender, CancellableEventArgs args, string? name = null);

    /// <summary>
    ///     Dispatches a cancelable event.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data.</param>
    /// <param name="name">The optional name of the event.</param>
    /// <returns>A value indicating whether the cancelable event was cancelled.</returns>
    /// <remarks>See general remarks on the interface.</remarks>
    bool DispatchCancelable<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string? name = null)
        where TArgs : CancellableEventArgs;

    /// <summary>
    ///     Dispatches a cancelable event.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data.</param>
    /// <param name="name">The optional name of the event.</param>
    /// <returns>A value indicating whether the cancelable event was cancelled.</returns>
    /// <remarks>See general remarks on the interface.</remarks>
    bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string? name = null)
        where TArgs : CancellableEventArgs;

    /// <summary>
    ///     Dispatches an event.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data.</param>
    /// <param name="name">The optional name of the event.</param>
    /// <remarks>See general remarks on the interface.</remarks>
    void Dispatch(EventHandler eventHandler, object sender, EventArgs args, string? name = null);

    /// <summary>
    ///     Dispatches an event.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data.</param>
    /// <param name="name">The optional name of the event.</param>
    /// <remarks>See general remarks on the interface.</remarks>
    void Dispatch<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string? name = null);

    /// <summary>
    ///     Dispatches an event.
    /// </summary>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data.</param>
    /// <param name="name">The optional name of the event.</param>
    /// <remarks>See general remarks on the interface.</remarks>
    void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string? name = null);

    /// <summary>
    ///     Notifies the dispatcher that the scope is exiting.
    /// </summary>
    /// <param name="completed">A value indicating whether the scope completed.</param>
    void ScopeExit(bool completed);

    /// <summary>
    ///     Gets the collected events.
    /// </summary>
    /// <returns>The collected events.</returns>
    IEnumerable<IEventDefinition> GetEvents(EventDefinitionFilter filter);
}
