using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

public class DictionaryOverviewViewModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryOverviewDisplay" /> class.
    /// </summary>
    public DictionaryOverviewViewModel() => Translations = new List<DictionaryTranslationOverviewViewModel>();

    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the level.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    ///     Sets the translations.
    /// </summary>
    public List<DictionaryTranslationOverviewViewModel> Translations { get; }
}
