namespace Umbraco.Cms.Core.Models.Membership.Permissions;

/// <summary>
///     Represents a granular permission for an element container (folder) node.
/// </summary>
public class ElementContainerGranularPermission : INodeGranularPermission
{
    /// <summary>
    ///     The context type identifier for element container permissions.
    /// </summary>
    public const string ContextType = "ElementContainer";

    /// <inheritdoc />
    public required Guid Key { get; set; }

    /// <inheritdoc />
    public string Context => ContextType;

    /// <inheritdoc />
    public required string Permission { get; set; }

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

        return Equals((ElementContainerGranularPermission)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Key, Permission);

    /// <summary>
    ///     Determines whether this instance is equal to another <see cref="ElementContainerGranularPermission" />.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    protected bool Equals(ElementContainerGranularPermission other) => Key.Equals(other.Key) && Permission == other.Permission;
}
