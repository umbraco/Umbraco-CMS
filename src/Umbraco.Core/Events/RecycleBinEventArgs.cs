namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for recycle bin operations.
/// </summary>
public class RecycleBinEventArgs : CancellableEventArgs, IEquatable<RecycleBinEventArgs>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RecycleBinEventArgs" /> class.
    /// </summary>
    /// <param name="nodeObjectType">The object type identifier of the items in the recycle bin.</param>
    /// <param name="eventMessages">The event messages.</param>
    public RecycleBinEventArgs(Guid nodeObjectType, EventMessages eventMessages)
        : base(true, eventMessages) =>
        NodeObjectType = nodeObjectType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecycleBinEventArgs" /> class.
    /// </summary>
    /// <param name="nodeObjectType">The object type identifier of the items in the recycle bin.</param>
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

    /// <summary>
    ///     Determines whether two <see cref="RecycleBinEventArgs" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(RecycleBinEventArgs left, RecycleBinEventArgs right) => Equals(left, right);

    /// <inheritdoc />
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

        return Equals((RecycleBinEventArgs)obj);
    }

    /// <inheritdoc />
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

    /// <summary>
    ///     Determines whether two <see cref="RecycleBinEventArgs" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(RecycleBinEventArgs left, RecycleBinEventArgs right) => !Equals(left, right);
}
