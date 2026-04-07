using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language;

/// <summary>
/// Serves as the base class for view models that represent language information in the management API.
/// </summary>
public class LanguageModelBase
{
    /// <summary>
    /// Gets or sets the name of the language.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default language.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the language is mandatory.
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Gets or sets the ISO code of the fallback language to use if a translation is unavailable.
    /// </summary>
    public string? FallbackIsoCode { get; set; }
}
