namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the User Profile interface
/// </summary>
public interface IProfile
{
    /// <summary>
    ///     Gets the unique identifier for the profile.
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the name of the profile.
    /// </summary>
    string? Name { get; }
}
