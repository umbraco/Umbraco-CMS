using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Core.Models.Membership;

public class ReadOnlyUserGroup : IReadOnlyUserGroup, IEquatable<ReadOnlyUserGroup>
{
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
    {
        Name = name ?? string.Empty;
        Icon = icon;
        Id = id;
        Key = key;
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

    public int Id { get; }

    public Guid Key { get; }

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

    public IEnumerable<int> AllowedLanguages { get; private set; }

    public ISet<string> Permissions { get; private set; }

    public ISet<IGranularPermission> GranularPermissions { get; private set; }

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
