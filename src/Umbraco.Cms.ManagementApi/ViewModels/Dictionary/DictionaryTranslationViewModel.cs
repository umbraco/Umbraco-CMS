using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

/// <summary>
///     The dictionary translation display model
/// </summary>
[DataContract(Name = "dictionaryTranslation", Namespace = "")]
public class DictionaryTranslationViewModel
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "key")]
    public Guid Key { get; set; }
    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    [DataMember(Name = "displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the ISO code.
    /// </summary>
    [DataMember(Name = "isoCode")]
    public string? IsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the translation.
    /// </summary>
    [DataMember(Name = "translation")]
    public string Translation { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the language id.
    /// </summary>
    [DataMember(Name = "languageId")]
    public int LanguageId { get; set; }
}
