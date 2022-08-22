namespace Umbraco.Cms.Core.Events;

[SupersedeEvent(typeof(SaveEventArgs<>))]
[SupersedeEvent(typeof(PublishEventArgs<>))]
[SupersedeEvent(typeof(MoveEventArgs<>))]
[SupersedeEvent(typeof(CopyEventArgs<>))]
public class DeleteEventArgs<TEntity> : CancellableEnumerableObjectEventArgs<TEntity>,
    IEquatable<DeleteEventArgs<TEntity>>, IDeletingMediaFilesEventArgs
{
    /// <summary>
    ///     Constructor accepting multiple entities that are used in the delete operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    /// <param name="eventMessages"></param>
    public DeleteEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages) => MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the delete operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="eventMessages"></param>
    public DeleteEventArgs(IEnumerable<TEntity> eventObject, EventMessages eventMessages)
        : base(
        eventObject,
        eventMessages) => MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="eventMessages"></param>
    public DeleteEventArgs(TEntity eventObject, EventMessages eventMessages)
        : base(new List<TEntity> { eventObject }, eventMessages) =>
        MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    /// <param name="eventMessages"></param>
    public DeleteEventArgs(TEntity eventObject, bool canCancel, EventMessages eventMessages)
        : base(new List<TEntity> { eventObject }, canCancel, eventMessages) =>
        MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the delete operation
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    public DeleteEventArgs(IEnumerable<TEntity> eventObject, bool canCancel)
        : base(eventObject, canCancel) =>
        MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting multiple entities that are used in the delete operation
    /// </summary>
    /// <param name="eventObject"></param>
    public DeleteEventArgs(IEnumerable<TEntity> eventObject)
        : base(eventObject) =>
        MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    public DeleteEventArgs(TEntity eventObject)
        : base(new List<TEntity> { eventObject }) =>
        MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Constructor accepting a single entity instance
    /// </summary>
    /// <param name="eventObject"></param>
    /// <param name="canCancel"></param>
    public DeleteEventArgs(TEntity eventObject, bool canCancel)
        : base(new List<TEntity> { eventObject }, canCancel) =>
        MediaFilesToDelete = new List<string>();

    /// <summary>
    ///     Returns all entities that were deleted during the operation
    /// </summary>
    public IEnumerable<TEntity> DeletedEntities
    {
        get => EventObject ?? Enumerable.Empty<TEntity>();
        set => EventObject = value;
    }

    /// <summary>
    ///     A list of media files that can be added to during a deleted operation for which Umbraco will ensure are removed
    /// </summary>
    public List<string> MediaFilesToDelete { get; }

    public static bool operator ==(DeleteEventArgs<TEntity> left, DeleteEventArgs<TEntity> right) =>
        Equals(left, right);

    public bool Equals(DeleteEventArgs<TEntity>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && MediaFilesToDelete.SequenceEqual(other.MediaFilesToDelete);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((DeleteEventArgs<TEntity>)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (base.GetHashCode() * 397) ^ MediaFilesToDelete.GetHashCode();
        }
    }

    public static bool operator !=(DeleteEventArgs<TEntity> left, DeleteEventArgs<TEntity> right) =>
        !Equals(left, right);
}

public class DeleteEventArgs : CancellableEventArgs, IEquatable<DeleteEventArgs>
{
    public DeleteEventArgs(int id, bool canCancel, EventMessages eventMessages)
        : base(canCancel, eventMessages) =>
        Id = id;

    public DeleteEventArgs(int id, bool canCancel)
        : base(canCancel) =>
        Id = id;

    public DeleteEventArgs(int id) => Id = id;

    /// <summary>
    ///     Gets the Id of the object being deleted.
    /// </summary>
    public int Id { get; }

    public static bool operator ==(DeleteEventArgs left, DeleteEventArgs right) => Equals(left, right);

    public bool Equals(DeleteEventArgs? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((DeleteEventArgs)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (base.GetHashCode() * 397) ^ Id;
        }
    }

    public static bool operator !=(DeleteEventArgs left, DeleteEventArgs right) => !Equals(left, right);
}
