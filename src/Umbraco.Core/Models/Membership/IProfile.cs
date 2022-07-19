namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the User Profile interface
/// </summary>
public interface IProfile
{
    int Id { get; }

    string? Name { get; }
}
