namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the contract for user-specific data storage.
/// </summary>
public interface IUserData
{
    /// <summary>
    ///     Gets or sets the unique key for the user data entry.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the user this data belongs to.
    /// </summary>
    public Guid UserKey { get; set; }

    /// <summary>
    ///     Gets or sets the group category for the user data.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    ///     Gets or sets the identifier for the user data within the group.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    ///     Gets or sets the value of the user data.
    /// </summary>
    public string Value { get; set; }
}
