namespace Umbraco.Cms.Core.Events;

public class SaveEventArgs<TEntity> : CancellableEnumerableObjectEventArgs<TEntity>
{
    /// <summary>
    ///     Constructor accepting multiple entities that are used in the saving operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    /// <param name="messages"></param>
    /// <param name="additionalData"></param>
    public SaveEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(eventObject, canCancel, messages, additionalData)
    {
    }

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the saving operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    /// <param name="eventMessages"></param>
    public SaveEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
    }

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the saving operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="eventMessages"></param>
    public SaveEventArgs(IEnumerable<TEntity> eventObject, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
    }

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    /// <param name="messages"></param>
    /// <param name="additionalData"></param>
    public SaveEventArgs(TEntity eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(new List<TEntity> { eventObject }, canCancel, messages, additionalData)
    {
    }

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="eventMessages"></param>
    public SaveEventArgs(TEntity eventObject, EventMessages eventMessages)
        : base(new List<TEntity> { eventObject }, eventMessages)
    {
    }

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    /// <param name="eventMessages"></param>
    public SaveEventArgs(TEntity eventObject, bool canCancel, EventMessages eventMessages)
        : base(new List<TEntity> { eventObject }, canCancel, eventMessages)
    {
    }

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the saving operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    public SaveEventArgs(IEnumerable<TEntity> eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the saving operation
    /// </summary>
    /// <param name="eventObject"></param>
    public SaveEventArgs(IEnumerable<TEntity> eventObject)
        : base(eventObject)
    {
    }

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    public SaveEventArgs(TEntity eventObject)
        : base(new List<TEntity> { eventObject })
    {
    }

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    public SaveEventArgs(TEntity eventObject, bool canCancel)
        : base(new List<TEntity> { eventObject }, canCancel)
    {
    }

    /// <summary>
    ///     Returns all entities that were saved during the operation
    /// </summary>
    public IEnumerable<TEntity>? SavedEntities => EventObject;
}
