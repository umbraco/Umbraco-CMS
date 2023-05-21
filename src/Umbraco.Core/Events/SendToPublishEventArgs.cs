namespace Umbraco.Cms.Core.Events;

public class SendToPublishEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
{
    public SendToPublishEventArgs(TEntity eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    public SendToPublishEventArgs(TEntity eventObject)
        : base(eventObject)
    {
    }

    /// <summary>
    ///     The entity being sent to publish
    /// </summary>
    public TEntity? Entity => EventObject;
}
