using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language;

/// <summary>
/// Represents a response model containing information about a language in the management API.
/// </summary>
public class LanguageResponseModel : LanguageModelBase
{
    /// <summary>
    /// Gets or sets the ISO code of the language.
    /// </summary>
    [Required]
    public string IsoCode { get; set; } = string.Empty;
}
