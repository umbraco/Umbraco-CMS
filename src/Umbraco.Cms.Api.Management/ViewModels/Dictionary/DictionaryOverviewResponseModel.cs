namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryOverviewResponseModel
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the parent key.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     Sets the translations.
    /// </summary>
    public IEnumerable<string> TranslatedIsoCodes { get; set; } = Array.Empty<string>();
}
