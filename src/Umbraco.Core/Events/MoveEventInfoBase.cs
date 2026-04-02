namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Provides a base class for move event information.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being moved.</typeparam>
public abstract class MoveEventInfoBase<TEntity> : IEquatable<MoveEventInfoBase<TEntity>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MoveEventInfoBase{TEntity}" /> class.
    /// </summary>
    /// <param name="entity">The entity being moved.</param>
    /// <param name="originalPath">The original path of the entity.</param>
    public MoveEventInfoBase(TEntity entity, string originalPath)
    {
        Entity = entity;
        OriginalPath = originalPath;
    }

    /// <summary>
    ///     Gets or sets the entity being moved.
    /// </summary>
    public TEntity Entity { get; set; }

    /// <summary>
    ///     Gets or sets the original path of the entity before the move.
    /// </summary>
    public string OriginalPath { get; set; }

    /// <inheritdoc />
    public bool Equals(MoveEventInfoBase<TEntity>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetType() != this.GetType())
        {
            return false;
        }

        return EqualityComparer<TEntity>.Default.Equals(Entity, other.Entity) && OriginalPath == other.OriginalPath;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals((MoveEventInfoBase<TEntity>?) obj);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Entity, OriginalPath);
}
