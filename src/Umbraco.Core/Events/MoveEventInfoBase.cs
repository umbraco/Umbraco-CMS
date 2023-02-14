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

        return EqualityComparer<TEntity>.Default.Equals(Entity, other.Entity) && OriginalPath == other.OriginalPath;
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

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((MoveEventInfoBase<TEntity>) obj);
    }

    public override int GetHashCode() => HashCode.Combine(Entity, OriginalPath);
}
