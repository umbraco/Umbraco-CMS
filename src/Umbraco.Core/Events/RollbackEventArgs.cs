namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for rollback operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being rolled back.</typeparam>
public class RollbackEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RollbackEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The entity being rolled back.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    public RollbackEventArgs(TEntity eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RollbackEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The entity being rolled back.</param>
    public RollbackEventArgs(TEntity eventObject)
        : base(eventObject)
    {
    }

    /// <summary>
    ///     The entity being rolledback
    /// </summary>
    public TEntity? Entity => EventObject;
}
