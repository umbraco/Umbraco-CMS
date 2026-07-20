namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents a user profile containing basic user information.
/// </summary>
public class UserProfile : IProfile, IEquatable<UserProfile>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserProfile" /> class.
    /// </summary>
    /// <param name="id">The unique identifier for the profile.</param>
    /// <param name="name">The name of the profile.</param>
    public UserProfile(int id, string? name)
    {
        Id = id;
        Name = name;
    }

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public string? Name { get; }

    /// <summary>
    ///     Determines whether two <see cref="UserProfile" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(UserProfile left, UserProfile right) => Equals(left, right);

    /// <summary>
    ///     Determines whether two <see cref="UserProfile" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(UserProfile left, UserProfile right) => Equals(left, right) == false;

    /// <inheritdoc />
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

        return Equals((UserProfile)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id;
}
