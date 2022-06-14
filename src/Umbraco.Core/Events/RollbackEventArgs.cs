namespace Umbraco.Cms.Core.Events;

public class RollbackEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
{
    public RollbackEventArgs(TEntity eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    public RollbackEventArgs(TEntity eventObject)
        : base(eventObject)
    {
    }

    /// <summary>
    ///     The entity being rolledback
    /// </summary>
    public TEntity? Entity => EventObject;
}
