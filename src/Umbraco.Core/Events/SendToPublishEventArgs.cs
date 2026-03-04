namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for send to publish operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being sent to publish.</typeparam>
public class SendToPublishEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SendToPublishEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The entity being sent to publish.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    public SendToPublishEventArgs(TEntity eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SendToPublishEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The entity being sent to publish.</param>
    public SendToPublishEventArgs(TEntity eventObject)
        : base(eventObject)
    {
    }

    /// <summary>
    ///     The entity being sent to publish
    /// </summary>
    public TEntity? Entity => EventObject;
}
