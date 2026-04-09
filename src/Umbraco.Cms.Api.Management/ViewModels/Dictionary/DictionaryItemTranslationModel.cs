using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Represents a translation for a dictionary item.
/// </summary>
public class DictionaryItemTranslationModel
{
    /// <summary>
    /// Gets or sets the ISO code for the dictionary item translation.
    /// </summary>
    [Required]
    public string IsoCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translation text for the dictionary item.
    /// </summary>
    public string Translation { get; set; } = string.Empty;
}
