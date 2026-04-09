namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for move operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being moved.</typeparam>
public class MoveEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>, IEquatable<MoveEventArgs<TEntity>>
{
    private IEnumerable<MoveEventInfo<TEntity>>? _moveInfoCollection;

    /// <summary>
    ///     Constructor accepting a collection of MoveEventInfo objects
    /// </summary>
    /// <param name="canCancel"></param>
    /// <param name="eventMessages"></param>
    /// <param name="moveInfo">
    ///     A collection of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
    /// </param>
    public MoveEventArgs(bool canCancel, EventMessages eventMessages, params MoveEventInfo<TEntity>[] moveInfo)
        : base(default, canCancel, eventMessages)
    {
        if (moveInfo.FirstOrDefault() is null)
        {
            throw new ArgumentException("moveInfo argument must contain at least one item");
        }

        MoveInfoCollection = moveInfo;

        // assign the legacy props
        EventObject = moveInfo.First().Entity;
    }

    /// <summary>
    ///     Constructor accepting a collection of MoveEventInfo objects
    /// </summary>
    /// <param name="eventMessages"></param>
    /// <param name="moveInfo">
    ///     A collection of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
    /// </param>
    public MoveEventArgs(EventMessages eventMessages, params MoveEventInfo<TEntity>[] moveInfo)
        : base(default, eventMessages)
    {
        if (moveInfo.FirstOrDefault() is null)
        {
            throw new ArgumentException("moveInfo argument must contain at least one item");
        }

        MoveInfoCollection = moveInfo;

        // assign the legacy props
        EventObject = moveInfo.First().Entity;
    }

    /// <summary>
    ///     Constructor accepting a collection of MoveEventInfo objects
    /// </summary>
    /// <param name="canCancel"></param>
    /// <param name="moveInfo">
    ///     A collection of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
    /// </param>
    public MoveEventArgs(bool canCancel, params MoveEventInfo<TEntity>[] moveInfo)
        : base(default, canCancel)
    {
        if (moveInfo.FirstOrDefault() is null)
        {
            throw new ArgumentException("moveInfo argument must contain at least one item");
        }

        MoveInfoCollection = moveInfo;

        // assign the legacy props
        EventObject = moveInfo.First().Entity;
    }

    /// <summary>
    ///     Constructor accepting a collection of MoveEventInfo objects
    /// </summary>
    /// <param name="moveInfo">
    ///     A collection of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
    /// </param>
    public MoveEventArgs(params MoveEventInfo<TEntity>[] moveInfo)
        : base(default)
    {
        if (moveInfo.FirstOrDefault() is null)
        {
            throw new ArgumentException("moveInfo argument must contain at least one item");
        }

        MoveInfoCollection = moveInfo;

        // assign the legacy props
        EventObject = moveInfo.First().Entity;
    }

    /// <summary>
    ///     Gets all MoveEventInfo objects used to create the object
    /// </summary>
    public IEnumerable<MoveEventInfo<TEntity>>? MoveInfoCollection
    {
        get => _moveInfoCollection;
        set
        {
            MoveEventInfo<TEntity>? first = value?.FirstOrDefault();
            if (first is null)
            {
                throw new InvalidOperationException("MoveInfoCollection must have at least one item");
            }

            _moveInfoCollection = value;

            // assign the legacy props
            EventObject = first.Entity;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="MoveEventArgs{TEntity}" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MoveEventArgs<TEntity> left, MoveEventArgs<TEntity> right) => Equals(left, right);

    /// <inheritdoc />
    public bool Equals(MoveEventArgs<TEntity>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && (MoveInfoCollection?.Equals(other.MoveInfoCollection) ?? false);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
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

        return Equals((MoveEventArgs<TEntity>)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            if (MoveInfoCollection is not null)
            {
                return (base.GetHashCode() * 397) ^ MoveInfoCollection.GetHashCode();
            }

            return base.GetHashCode() * 397;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="MoveEventArgs{TEntity}" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MoveEventArgs<TEntity> left, MoveEventArgs<TEntity> right) => !Equals(left, right);
}
