namespace Umbraco.Cms.Api.Management.ViewModels.UserData;

/// <summary>
/// Represents user data in the Umbraco CMS Management API.
/// </summary>
public class UserDataViewModel
{
    /// <summary>
    /// Gets or sets the group name associated with the user.
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the user data.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the string value associated with the user data entry.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
