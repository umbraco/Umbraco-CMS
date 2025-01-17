namespace Umbraco.Cms.Core.Events;

public class MoveEventInfo<TEntity> : MoveEventInfoBase<TEntity>
{
    public MoveEventInfo(TEntity entity, string originalPath, int newParentId, Guid? newParentKey)
        : base(entity, originalPath)
    {
        NewParentId = newParentId;
        NewParentKey = newParentKey;
    }

    public MoveEventInfo(TEntity entity, string originalPath, int newParentId) : this(entity, originalPath, newParentId, null)
    {
    }

    [Obsolete("Please use NewParentKey instead, scheduled for removal in V15")]
    public int NewParentId { get; set; }

    public Guid? NewParentKey { get; }

    public static bool operator ==(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right) => Equals(left, right);

    public override bool Equals(object? obj) => Equals((MoveEventInfo<TEntity>?)obj);

    public bool Equals(MoveEventInfo<TEntity>? other) => NewParentId == other?.NewParentId && NewParentKey == other.NewParentKey && base.Equals(other);

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
