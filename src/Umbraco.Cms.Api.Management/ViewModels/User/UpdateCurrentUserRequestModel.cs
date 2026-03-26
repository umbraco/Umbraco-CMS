namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model for updating information about a current user.
/// </summary>
public class UpdateCurrentUserRequestModel
{
    /// <summary>
    /// Gets or sets the ISO code of the user's language.
    /// </summary>
    public string LanguageIsoCode { get; set; } = string.Empty;
}
