using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Base model for a dictionary item in the Umbraco CMS Management API.
/// </summary>
public class DictionaryItemModelBase
{
    /// <summary>
    /// Gets or sets the name of the dictionary item.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of translations for the dictionary item.
    /// </summary>
    public IEnumerable<DictionaryItemTranslationModel> Translations { get; set; } = Enumerable.Empty<DictionaryItemTranslationModel>();
}
