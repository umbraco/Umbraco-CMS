namespace Umbraco.Cms.Core.Models;

/// <summary>
///  Represents the model used for updating a current user.
/// </summary>
public class CurrentUserUpdateModel
{
    /// <summary>
    ///     Gets or sets the ISO code of the user's preferred language.
    /// </summary>
    public required string LanguageIsoCode { get; set; }
}
