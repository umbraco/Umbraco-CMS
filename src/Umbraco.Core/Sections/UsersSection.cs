namespace Umbraco.Cms.Core.Sections;

/// <summary>
///     Defines the back office users section
/// </summary>
public class UsersSection : ISection
{
    /// <inheritdoc />
    public string Alias => Constants.Applications.Users;

    /// <inheritdoc />
    public string Name => "Users";
}
