using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language;

/// <summary>
/// Represents the API request model for creating a new language in the system.
/// </summary>
public class CreateLanguageRequestModel : LanguageModelBase
{
    /// <summary>
    /// Gets or sets the ISO code of the language.
    /// </summary>
    [Required]
    public string IsoCode { get; set; } = string.Empty;
}
