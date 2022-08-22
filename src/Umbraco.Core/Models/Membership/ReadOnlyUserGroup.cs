namespace Umbraco.Cms.Core.Models.Membership;

public class ReadOnlyUserGroup : IReadOnlyUserGroup, IEquatable<ReadOnlyUserGroup>
{
    public ReadOnlyUserGroup(
        int id,
        string? name,
        string? icon,
        int? startContentId,
        int? startMediaId,
        string? alias,
        IEnumerable<int> allowedLanguages,
        IEnumerable<string> allowedSections,
        IEnumerable<string>? permissions,
        bool hasAccessToAllLanguages)
    {
        Name = name ?? string.Empty;
        Icon = icon;
        Id = id;
        Alias = alias ?? string.Empty;
        AllowedLanguages = allowedLanguages.ToArray();
        AllowedSections = allowedSections.ToArray();
        Permissions = permissions?.ToArray();

        // Zero is invalid and will be treated as Null
        StartContentId = startContentId == 0 ? null : startContentId;
        StartMediaId = startMediaId == 0 ? null : startMediaId;
        HasAccessToAllLanguages = hasAccessToAllLanguages;
    }

    [Obsolete("please use ctor that takes allowedActions & hasAccessToAllLanguages instead, scheduled for removal in v12")]
    public ReadOnlyUserGroup(
        int id,
        string? name,
        string? icon,
        int? startContentId,
        int? startMediaId,
        string? alias,
        IEnumerable<string> allowedSections,
        IEnumerable<string>? permissions)
    : this(id, name, icon, startContentId, startMediaId, alias, Enumerable.Empty<int>(), allowedSections, permissions, true)
    {
    }

    public int Id { get; }

    public bool Equals(ReadOnlyUserGroup? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Alias, other.Alias);
    }

    public string Name { get; }

    public string? Icon { get; }

    public int? StartContentId { get; }

    public int? StartMediaId { get; }

    public string Alias { get; }

    public bool HasAccessToAllLanguages { get; set; }

    /// <summary>
    ///     The set of default permissions
    /// </summary>
    /// <remarks>
    ///     By default each permission is simply a single char but we've made this an enumerable{string} to support a more
    ///     flexible permissions structure in the future.
    /// </remarks>
    public IEnumerable<string>? Permissions { get; set; }

    public IEnumerable<int> AllowedLanguages { get; private set; }
    public IEnumerable<string> AllowedSections { get; private set; }

    public static bool operator ==(ReadOnlyUserGroup left, ReadOnlyUserGroup right) => Equals(left, right);

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

        return Equals((ReadOnlyUserGroup)obj);
    }

    public override int GetHashCode() => Alias?.GetHashCode() ?? base.GetHashCode();

    public static bool operator !=(ReadOnlyUserGroup left, ReadOnlyUserGroup right) => !Equals(left, right);
}
