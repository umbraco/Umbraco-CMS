namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents information about a single entity move operation.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being moved.</typeparam>
public class MoveEventInfo<TEntity> : MoveEventInfoBase<TEntity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MoveEventInfo{TEntity}" /> class.
    /// </summary>
    /// <param name="entity">The entity being moved.</param>
    /// <param name="originalPath">The original path of the entity.</param>
    /// <param name="newParentId">The identifier of the new parent.</param>
    /// <param name="newParentKey">The unique identifier of the new parent.</param>
    public MoveEventInfo(TEntity entity, string originalPath, int newParentId, Guid? newParentKey)
        : base(entity, originalPath)
    {
        NewParentId = newParentId;
        NewParentKey = newParentKey;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MoveEventInfo{TEntity}" /> class.
    /// </summary>
    /// <param name="entity">The entity being moved.</param>
    /// <param name="originalPath">The original path of the entity.</param>
    /// <param name="newParentId">The identifier of the new parent.</param>
    public MoveEventInfo(TEntity entity, string originalPath, int newParentId) : this(entity, originalPath, newParentId, null)
    {
    }

    /// <summary>
    ///     Gets or sets the identifier of the new parent.
    /// </summary>
    [Obsolete("Please use NewParentKey instead, scheduled for removal in V15")]
    public int NewParentId { get; set; }

    /// <summary>
    ///     Gets the unique identifier of the new parent.
    /// </summary>
    public Guid? NewParentKey { get; }

    /// <summary>
    ///     Determines whether two <see cref="MoveEventInfo{TEntity}" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right) => Equals(left, right);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals((MoveEventInfo<TEntity>?)obj);

    /// <summary>
    ///     Determines whether this instance is equal to another <see cref="MoveEventInfo{TEntity}" /> instance.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(MoveEventInfo<TEntity>? other) => NewParentId == other?.NewParentId && NewParentKey == other.NewParentKey && base.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Entity is not null
                ? EqualityComparer<TEntity>.Default.GetHashCode(Entity)
                : base.GetHashCode();
            hashCode = (hashCode * 397) ^ NewParentId;
            hashCode = (hashCode * 397) ^ OriginalPath.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="MoveEventInfo{TEntity}" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right) => !Equals(left, right);
}
