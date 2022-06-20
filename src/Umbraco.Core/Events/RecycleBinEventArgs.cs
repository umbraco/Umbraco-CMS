namespace Umbraco.Cms.Core.Events;

public class RecycleBinEventArgs : CancellableEventArgs, IEquatable<RecycleBinEventArgs>
{
    public RecycleBinEventArgs(Guid nodeObjectType, EventMessages eventMessages)
        : base(true, eventMessages) =>
        NodeObjectType = nodeObjectType;

    public RecycleBinEventArgs(Guid nodeObjectType)
        : base(true) =>
        NodeObjectType = nodeObjectType;

    /// <summary>
    ///     Gets the Id of the node object type of the items
    ///     being deleted from the Recycle Bin.
    /// </summary>
    public Guid NodeObjectType { get; }

    /// <summary>
    ///     Boolean indicating whether the Recycle Bin was emptied successfully
    /// </summary>
    public bool RecycleBinEmptiedSuccessfully { get; set; }

    /// <summary>
    ///     Boolean indicating whether this event was fired for the Content's Recycle Bin.
    /// </summary>
    public bool IsContentRecycleBin => NodeObjectType == Constants.ObjectTypes.Document;

    /// <summary>
    ///     Boolean indicating whether this event was fired for the Media's Recycle Bin.
    /// </summary>
    public bool IsMediaRecycleBin => NodeObjectType == Constants.ObjectTypes.Media;

    public static bool operator ==(RecycleBinEventArgs left, RecycleBinEventArgs right) => Equals(left, right);

    public bool Equals(RecycleBinEventArgs? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && NodeObjectType.Equals(other.NodeObjectType) &&
               RecycleBinEmptiedSuccessfully == other.RecycleBinEmptiedSuccessfully;
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

        return Equals((RecycleBinEventArgs)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ NodeObjectType.GetHashCode();
            hashCode = (hashCode * 397) ^ RecycleBinEmptiedSuccessfully.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator !=(RecycleBinEventArgs left, RecycleBinEventArgs right) => !Equals(left, right);
}
