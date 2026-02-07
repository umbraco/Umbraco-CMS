namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for copy operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being copied.</typeparam>
public class CopyEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>, IEquatable<CopyEventArgs<TEntity>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CopyEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="original">The original entity being copied.</param>
    /// <param name="copy">The copied entity.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    public CopyEventArgs(TEntity original, TEntity copy, bool canCancel, int parentId)
        : base(original, canCancel)
    {
        Copy = copy;
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CopyEventArgs{TEntity}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The original entity being copied.</param>
    /// <param name="copy">The copied entity.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    public CopyEventArgs(TEntity eventObject, TEntity copy, int parentId)
        : base(eventObject)
    {
        Copy = copy;
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CopyEventArgs{TEntity}" /> class.
    /// </summary>
    /// <param name="eventObject">The original entity being copied.</param>
    /// <param name="copy">The copied entity.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    /// <param name="relateToOriginal">A value indicating whether to create a relation to the original entity.</param>
    public CopyEventArgs(TEntity eventObject, TEntity copy, bool canCancel, int parentId, bool relateToOriginal)
        : base(eventObject, canCancel)
    {
        Copy = copy;
        ParentId = parentId;
        RelateToOriginal = relateToOriginal;
    }

    /// <summary>
    ///     The copied entity
    /// </summary>
    public TEntity Copy { get; set; }

    /// <summary>
    ///     The original entity
    /// </summary>
    public TEntity? Original => EventObject;

    /// <summary>
    ///     Gets or Sets the Id of the objects new parent.
    /// </summary>
    public int ParentId { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether to create a relation to the original entity.
    /// </summary>
    public bool RelateToOriginal { get; set; }

    /// <summary>
    ///     Determines whether two <see cref="CopyEventArgs{TEntity}" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(CopyEventArgs<TEntity> left, CopyEventArgs<TEntity> right) => Equals(left, right);

    /// <inheritdoc />
    public bool Equals(CopyEventArgs<TEntity>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && EqualityComparer<TEntity>.Default.Equals(Copy, other.Copy) &&
               ParentId == other.ParentId && RelateToOriginal == other.RelateToOriginal;
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

        return Equals((CopyEventArgs<TEntity>)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            if (Copy is not null)
            {
                hashCode = (hashCode * 397) ^ EqualityComparer<TEntity>.Default.GetHashCode(Copy);
            }

            hashCode = (hashCode * 397) ^ ParentId;
            hashCode = (hashCode * 397) ^ RelateToOriginal.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="CopyEventArgs{TEntity}" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(CopyEventArgs<TEntity> left, CopyEventArgs<TEntity> right) => !Equals(left, right);
}
