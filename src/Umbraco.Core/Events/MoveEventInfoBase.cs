namespace Umbraco.Cms.Core.Events;

public abstract class MoveEventInfoBase<TEntity> : IEquatable<MoveEventInfoBase<TEntity>>
{
    public MoveEventInfoBase(TEntity entity, string originalPath)
    {
        Entity = entity;
        OriginalPath = originalPath;
    }

    public TEntity Entity { get; set; }

    public string OriginalPath { get; set; }

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

    public override bool Equals(object? obj) => Equals((MoveEventInfoBase<TEntity>?) obj);

    public override int GetHashCode() => HashCode.Combine(Entity, OriginalPath);
}
