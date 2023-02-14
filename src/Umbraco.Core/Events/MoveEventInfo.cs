namespace Umbraco.Cms.Core.Events;

public class MoveEventInfo<TEntity> : MoveEventInfoBase<TEntity>
{
    public MoveEventInfo(TEntity entity, string originalPath, int newParentId, Guid? newParentKey)
        : base(entity, originalPath)
    {
        NewParentId = newParentId;
        NewParentKey = newParentKey;
    }

    public MoveEventInfo(TEntity entity, string originalPath, int newParentId) : this(entity, originalPath, newParentId,
        null)
    {
    }

    [Obsolete("Please use NewParentKey instead, scheduled for removal in V15")]
    public int NewParentId { get; set; }

    public Guid? NewParentKey { get; }

    public static bool operator ==(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right) => Equals(left, right);

    public bool Equals(MoveEventInfo<TEntity>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<TEntity>.Default.Equals(
                   Entity,
                   other.Entity) &&
               NewParentId == other.NewParentId &&
               NewParentKey == other.NewParentKey &&
               string.Equals(OriginalPath, other.OriginalPath);
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

        return Equals((MoveEventInfo<TEntity>)obj);
    }

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

    public static bool operator !=(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right) => !Equals(left, right);
}
