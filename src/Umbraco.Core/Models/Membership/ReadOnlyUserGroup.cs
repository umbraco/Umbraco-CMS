using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents a read-only implementation of a user group providing basic information.
/// </summary>
public class ReadOnlyUserGroup : IReadOnlyUserGroup, IEquatable<ReadOnlyUserGroup>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadOnlyUserGroup" /> class.
    /// </summary>
    /// <param name="id">The unique identifier for the user group.</param>
    /// <param name="key">The unique key for the user group.</param>
    /// <param name="name">The name of the user group.</param>
    /// <param name="icon">The icon for the user group.</param>
    /// <param name="startContentId">The starting content node identifier.</param>
    /// <param name="startMediaId">The starting media node identifier.</param>
    /// <param name="alias">The alias of the user group.</param>
    /// <param name="allowedLanguages">The collection of allowed language identifiers.</param>
    /// <param name="allowedSections">The collection of allowed section aliases.</param>
    /// <param name="permissions">The set of permissions.</param>
    /// <param name="granularPermissions">The set of granular permissions.</param>
    /// <param name="hasAccessToAllLanguages">Indicates whether the group has access to all languages.</param>
    [Obsolete("Please use the constructor that includes all parameters. Scheduled for removal in Umbraco 19.")]
    public ReadOnlyUserGroup(
        int id,
        Guid key,
        string? name,
        string? icon,
        int? startContentId,
        int? startMediaId,
        string? alias,
        IEnumerable<int> allowedLanguages,
        IEnumerable<string> allowedSections,
        ISet<string> permissions,
        ISet<IGranularPermission> granularPermissions,
        bool hasAccessToAllLanguages)
        : this(
            id,
            key,
            name,
            null,
            icon,
            startContentId,
            startMediaId,
            alias,
            allowedLanguages,
            allowedSections,
            permissions,
            granularPermissions,
            hasAccessToAllLanguages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadOnlyUserGroup" /> class.
    /// </summary>
    /// <param name="id">The unique identifier for the user group.</param>
    /// <param name="key">The unique key for the user group.</param>
    /// <param name="name">The name of the user group.</param>
    /// <param name="description">The description of the user group.</param>
    /// <param name="icon">The icon for the user group.</param>
    /// <param name="startContentId">The starting content node identifier.</param>
    /// <param name="startMediaId">The starting media node identifier.</param>
    /// <param name="alias">The alias of the user group.</param>
    /// <param name="allowedLanguages">The collection of allowed language identifiers.</param>
    /// <param name="allowedSections">The collection of allowed section aliases.</param>
    /// <param name="permissions">The set of permissions.</param>
    /// <param name="granularPermissions">The set of granular permissions.</param>
    /// <param name="hasAccessToAllLanguages">Indicates whether the group has access to all languages.</param>
    public ReadOnlyUserGroup(
        int id,
        Guid key,
        string? name,
        string? description,
        string? icon,
        int? startContentId,
        int? startMediaId,
        string? alias,
        IEnumerable<int> allowedLanguages,
        IEnumerable<string> allowedSections,
        ISet<string> permissions,
        ISet<IGranularPermission> granularPermissions,
        bool hasAccessToAllLanguages)
    {
        Id = id;
        Key = key;
        Name = name ?? string.Empty;
        Description = description;
        Icon = icon;
        Alias = alias ?? string.Empty;
        AllowedLanguages = allowedLanguages.ToArray();
        AllowedSections = allowedSections.ToArray();

        // Zero is invalid and will be treated as Null
        StartContentId = startContentId == 0 ? null : startContentId;
        StartMediaId = startMediaId == 0 ? null : startMediaId;
        HasAccessToAllLanguages = hasAccessToAllLanguages;
        Permissions = permissions;
        GranularPermissions = granularPermissions;
    }

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public Guid Key { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string? Description { get; }

    /// <inheritdoc />
    public string? Icon { get; }

    /// <inheritdoc />
    public int? StartContentId { get; }

    /// <inheritdoc />
    public int? StartMediaId { get; }

    /// <inheritdoc />
    public string Alias { get; }

    /// <inheritdoc />
    public bool HasAccessToAllLanguages { get; set; }

    /// <inheritdoc />
    public IEnumerable<int> AllowedLanguages { get; private set; }

    /// <inheritdoc />
    public ISet<string> Permissions { get; private set; }

    /// <inheritdoc />
    public ISet<IGranularPermission> GranularPermissions { get; private set; }

    /// <inheritdoc />
    public IEnumerable<string> AllowedSections { get; private set; }

    /// <summary>
    ///     Determines whether two <see cref="ReadOnlyUserGroup" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(ReadOnlyUserGroup left, ReadOnlyUserGroup right) => Equals(left, right);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
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

        return Equals((ReadOnlyUserGroup)obj);
    }

    /// <inheritdoc />
    public bool Equals(ReadOnlyUserGroup? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Alias, other.Alias);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Alias?.GetHashCode() ?? base.GetHashCode();

    /// <summary>
    ///     Determines whether two <see cref="ReadOnlyUserGroup" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(ReadOnlyUserGroup left, ReadOnlyUserGroup right) => !Equals(left, right);
}
