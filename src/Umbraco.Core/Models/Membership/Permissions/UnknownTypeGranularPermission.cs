namespace Umbraco.Cms.Core.Models.Membership.Permissions;

/// <summary>
///     Represents a granular permission with an unknown or unrecognized context type.
/// </summary>
public class UnknownTypeGranularPermission : IGranularPermission
{
    /// <inheritdoc />
    public required string Context { get; set; }

    /// <inheritdoc />
    public required string Permission { get; set; }

    /// <summary>
    ///     Determines whether this instance is equal to another <see cref="UnknownTypeGranularPermission" />.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    protected bool Equals(UnknownTypeGranularPermission other) => Context == other.Context && Permission == other.Permission;

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

        return Equals((UnknownTypeGranularPermission)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Context, Permission);
}
