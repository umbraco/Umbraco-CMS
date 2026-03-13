using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language.Item;

/// <summary>
/// Represents a response model for a language item in the Umbraco CMS Management API.
/// </summary>
public class LanguageItemResponseModel
{
    /// <summary>
    /// Gets or sets the name of the language.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ISO code of the language.
    /// </summary>
    [Required]
    public string IsoCode { get; set; } = string.Empty;
}
