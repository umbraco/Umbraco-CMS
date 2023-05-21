using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The dictionary translation overview display.
/// </summary>
[DataContract(Name = "dictionaryTranslation", Namespace = "")]
public class DictionaryOverviewTranslationDisplay
{
    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    [DataMember(Name = "displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether has translation.
    /// </summary>
    [DataMember(Name = "hasTranslation")]
    public bool HasTranslation { get; set; }
}
