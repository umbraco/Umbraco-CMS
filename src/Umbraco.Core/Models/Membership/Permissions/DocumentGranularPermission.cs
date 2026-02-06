namespace Umbraco.Cms.Core.Models.Membership.Permissions;

/// <summary>
///     Represents a granular permission for a document (content) node.
/// </summary>
public class DocumentGranularPermission : INodeGranularPermission
{
    /// <summary>
    ///     The context type identifier for document permissions.
    /// </summary>
    public const string ContextType = "Document";

    /// <inheritdoc />
    public required Guid Key { get; set; }

    /// <inheritdoc />
    public string Context => ContextType;

    /// <inheritdoc />
    public required string Permission { get; set; }

    /// <summary>
    ///     Determines whether this instance is equal to another <see cref="DocumentGranularPermission" />.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    protected bool Equals(DocumentGranularPermission other) => Key.Equals(other.Key) && Permission == other.Permission;

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

        return Equals((DocumentGranularPermission)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Key, Permission);
}
