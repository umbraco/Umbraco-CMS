using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

public class DictionaryTranslationViewModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("key")]
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the ISO code.
    /// </summary>
    [JsonPropertyName("isoCode")]
    public string? IsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the translation.
    /// </summary>
    [JsonPropertyName("translation")]
    public string Translation { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the language id.
    /// </summary>
    [JsonPropertyName("languageId")]
    public int LanguageId { get; set; }
}
