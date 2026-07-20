namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for new entity creation operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being created.</typeparam>
public class NewEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>, IEquatable<NewEventArgs<TEntity>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parentId">The identifier of the parent entity.</param>
    /// <param name="eventMessages">The event messages.</param>
    public NewEventArgs(TEntity eventObject, bool canCancel, string alias, int parentId, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
        Alias = alias;
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parent">The parent entity.</param>
    /// <param name="eventMessages">The event messages.</param>
    public NewEventArgs(TEntity eventObject, bool canCancel, string alias, TEntity? parent, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
        Alias = alias;
        Parent = parent;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parentId">The identifier of the parent entity.</param>
    /// <param name="eventMessages">The event messages.</param>
    public NewEventArgs(TEntity eventObject, string alias, int parentId, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
        Alias = alias;
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parent">The parent entity.</param>
    /// <param name="eventMessages">The event messages.</param>
    public NewEventArgs(TEntity eventObject, string alias, TEntity? parent, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
        Alias = alias;
        Parent = parent;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parentId">The identifier of the parent entity.</param>
    public NewEventArgs(TEntity eventObject, bool canCancel, string alias, int parentId)
        : base(eventObject, canCancel)
    {
        Alias = alias;
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parent">The parent entity.</param>
    public NewEventArgs(TEntity eventObject, bool canCancel, string alias, TEntity? parent)
        : base(eventObject, canCancel)
    {
        Alias = alias;
        Parent = parent;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parentId">The identifier of the parent entity.</param>
    public NewEventArgs(TEntity eventObject, string alias, int parentId)
        : base(eventObject)
    {
        Alias = alias;
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The entity being created.</param>
    /// <param name="alias">The alias of the entity.</param>
    /// <param name="parent">The parent entity.</param>
    public NewEventArgs(TEntity eventObject, string alias, TEntity? parent)
        : base(eventObject)
    {
        Alias = alias;
        Parent = parent;
    }

    /// <summary>
    ///     The entity being created
    /// </summary>
    public TEntity? Entity => EventObject;

    /// <summary>
    ///     Gets or Sets the Alias.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    ///     Gets or Sets the Id of the parent.
    /// </summary>
    public int ParentId { get; }

    /// <summary>
    ///     Gets or Sets the parent IContent object.
    /// </summary>
    public TEntity? Parent { get; }

    /// <summary>
    ///     Determines whether two <see cref="NewEventArgs{TEntity}" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(NewEventArgs<TEntity> left, NewEventArgs<TEntity> right) => Equals(left, right);

    /// <inheritdoc />
    public bool Equals(NewEventArgs<TEntity>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && string.Equals(Alias, other.Alias) &&
               EqualityComparer<TEntity>.Default.Equals(Parent, other.Parent) && ParentId == other.ParentId;
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

        return Equals((NewEventArgs<TEntity>?)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ Alias.GetHashCode();
            if (Parent is not null)
            {
                hashCode = (hashCode * 397) ^ EqualityComparer<TEntity>.Default.GetHashCode(Parent);
            }

            hashCode = (hashCode * 397) ^ ParentId;
            return hashCode;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="NewEventArgs{TEntity}" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(NewEventArgs<TEntity> left, NewEventArgs<TEntity> right) => !Equals(left, right);
}
