namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents user-specific data storage.
/// </summary>
public class UserData : IUserData
{
    /// <inheritdoc />
    public Guid Key { get; set; }

    /// <inheritdoc />
    public Guid UserKey { get; set; }

    /// <inheritdoc />
    public string Group { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Identifier { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Value { get; set; } = string.Empty;
}
