namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for delete operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being deleted.</typeparam>
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

    /// <summary>
    ///     Determines whether two <see cref="DeleteEventArgs{TEntity}" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(DeleteEventArgs<TEntity> left, DeleteEventArgs<TEntity> right) =>
        Equals(left, right);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (base.GetHashCode() * 397) ^ MediaFilesToDelete.GetHashCode();
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="DeleteEventArgs{TEntity}" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(DeleteEventArgs<TEntity> left, DeleteEventArgs<TEntity> right) =>
        !Equals(left, right);
}

/// <summary>
///     Represents event data for delete operations with an identifier.
/// </summary>
public class DeleteEventArgs : CancellableEventArgs, IEquatable<DeleteEventArgs>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteEventArgs" /> class.
    /// </summary>
    /// <param name="id">The identifier of the object being deleted.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="eventMessages">The event messages.</param>
    public DeleteEventArgs(int id, bool canCancel, EventMessages eventMessages)
        : base(canCancel, eventMessages) =>
        Id = id;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteEventArgs" /> class.
    /// </summary>
    /// <param name="id">The identifier of the object being deleted.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    public DeleteEventArgs(int id, bool canCancel)
        : base(canCancel) =>
        Id = id;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteEventArgs" /> class with cancellation enabled.
    /// </summary>
    /// <param name="id">The identifier of the object being deleted.</param>
    public DeleteEventArgs(int id) => Id = id;

    /// <summary>
    ///     Gets the Id of the object being deleted.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Determines whether two <see cref="DeleteEventArgs" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(DeleteEventArgs left, DeleteEventArgs right) => Equals(left, right);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (base.GetHashCode() * 397) ^ Id;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="DeleteEventArgs" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(DeleteEventArgs left, DeleteEventArgs right) => !Equals(left, right);
}
