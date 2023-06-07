namespace Umbraco.Cms.Core.Events;

public class CopyEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>, IEquatable<CopyEventArgs<TEntity>>
{
    public CopyEventArgs(TEntity original, TEntity copy, bool canCancel, int parentId)
        : base(original, canCancel)
    {
        Copy = copy;
        ParentId = parentId;
    }

    public CopyEventArgs(TEntity eventObject, TEntity copy, int parentId)
        : base(eventObject)
    {
        Copy = copy;
        ParentId = parentId;
    }

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

    public bool RelateToOriginal { get; set; }

    public static bool operator ==(CopyEventArgs<TEntity> left, CopyEventArgs<TEntity> right) => Equals(left, right);

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

    public static bool operator !=(CopyEventArgs<TEntity> left, CopyEventArgs<TEntity> right) => !Equals(left, right);
}
