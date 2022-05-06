namespace Umbraco.Cms.Core.Models.Membership;

public class UserProfile : IProfile, IEquatable<UserProfile>
{
    public UserProfile(int id, string? name)
    {
        Id = id;
        Name = name;
    }

    public bool Equals(UserProfile? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public int Id { get; }
    public string? Name { get; }

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

        return Equals((UserProfile)obj);
    }

    public override int GetHashCode() => Id;

    public static bool operator ==(UserProfile left, UserProfile right) => Equals(left, right);

    public static bool operator !=(UserProfile left, UserProfile right) => Equals(left, right) == false;
}
