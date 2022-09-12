using System.Runtime.Serialization;
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
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the id.
    /// </summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the level.
    /// </summary>
    [DataMember(Name = "level")]
    public int Level { get; set; }

    /// <summary>
    ///     Gets or sets the translations.
    /// </summary>
    [DataMember(Name = "translations")]
    public List<DictionaryTranslationOverviewViewModel> Translations { get; set; }
}
