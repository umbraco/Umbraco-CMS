namespace Umbraco.Cms.Core.Models.Membership;

public class ReadOnlyUserGroup : IReadOnlyUserGroup, IEquatable<ReadOnlyUserGroup>
{
    public ReadOnlyUserGroup(int id, string? name, string? icon, int? startContentId, int? startMediaId, string? alias, IEnumerable<string> allowedSections, IEnumerable<string>? permissions)
    {
        Name = name ?? string.Empty;
        Icon = icon;
        Id = id;
        Alias = alias ?? string.Empty;
        AllowedSections = allowedSections.ToArray();
        Permissions = permissions?.ToArray();

        // Zero is invalid and will be treated as Null
        StartContentId = startContentId == 0 ? null : startContentId;
        StartMediaId = startMediaId == 0 ? null : startMediaId;
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

    /// <summary>
    ///     The set of default permissions
    /// </summary>
    /// <remarks>
    ///     By default each permission is simply a single char but we've made this an enumerable{string} to support a more
    ///     flexible permissions structure in the future.
    /// </remarks>
    public IEnumerable<string>? Permissions { get; set; }

    public IEnumerable<string> AllowedSections { get; }

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
