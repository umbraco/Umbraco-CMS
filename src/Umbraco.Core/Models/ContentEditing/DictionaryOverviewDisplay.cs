using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The dictionary overview display.
/// </summary>
[DataContract(Name = "dictionary", Namespace = "")]
public class DictionaryOverviewDisplay
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryOverviewDisplay" /> class.
    /// </summary>
    public DictionaryOverviewDisplay() => Translations = new List<DictionaryOverviewTranslationDisplay>();

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
    public List<DictionaryOverviewTranslationDisplay> Translations { get; set; }
}
