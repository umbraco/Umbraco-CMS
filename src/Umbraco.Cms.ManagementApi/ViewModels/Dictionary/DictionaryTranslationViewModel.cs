namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

public class DictionaryTranslationViewModel
{
    public int Id { get; set; }

    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the ISO code.
    /// </summary>
    public string? IsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the translation.
    /// </summary>
    public string Translation { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the language id.
    /// </summary>
    public int LanguageId { get; set; }
}
